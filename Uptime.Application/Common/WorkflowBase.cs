using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public abstract class WorkflowBase<TContext>(IWorkflowService workflowService, ILogger<WorkflowBase<TContext>> logger) : IWorkflowMachine, IWorkflow<TContext>
    where TContext : class, IWorkflowContext, new()
{
    protected IStateMachine<WorkflowPhase, WorkflowTrigger> Machine = null!;

    protected WorkflowId WorkflowId;

    public TContext WorkflowContext { get; private set; } = new();

    public IWorkflowService WorkflowService { get; } = workflowService;
    
    private readonly AsyncLockHelper _asyncLockHelper = new();
    
    public virtual async Task<WorkflowPhase> StartAsync(IWorkflowPayload payload, CancellationToken cancellationToken = default)
    {
        InitializeStateMachine(WorkflowPhase.NotStarted);

        WorkflowId = await WorkflowService.CreateWorkflowInstanceAsync(payload);

        OnWorkflowActivated(payload);

        await Machine.FireAsync(WorkflowTrigger.Start);

        return await SaveWorkflowStateAsync();
    }

    /*
    private int _fireAsyncActiveCount;   

    public virtual async Task FireAsync(WorkflowTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default)
    {
        int activeCount = Interlocked.Increment(ref _fireAsyncActiveCount);
        if (activeCount > 1)
        {
            logger.LogWarning("FireAsync is being called concurrently! Active count: {ActiveCount}", activeCount);
        }

        try
        {
            await _fireAsyncLock.WaitAsync(cancellationToken);
            try
            {
                logger.LogInformation("Triggering workflow state change: {CurrentState} -> {Trigger}", Machine.CurrentState, trigger);
                await Machine.FireAsync(trigger);
                logger.LogInformation("New workflow state: {NewState}", Machine.CurrentState);

                if (autoCommit)
                {
                    await SaveWorkflowStateAsync();
                }
            }
            finally
            {
                _fireAsyncLock.Release();
            }
        }
        finally
        {
            Interlocked.Decrement(ref _fireAsyncActiveCount);
        }
    }
    */



    private readonly SemaphoreSlim _fireAsyncLock = new(1, 1);
    private readonly Queue<WorkflowTrigger> _triggerQueue = new();
    private bool _isProcessingQueue;
    
    public async Task FireAsync(WorkflowTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default)
    {
        lock (_triggerQueue)
        {
            _triggerQueue.Enqueue(trigger);
            logger.LogInformation("Trigger {Trigger} enqueued. Queue size: {QueueSize}", trigger, _triggerQueue.Count);

            if (_isProcessingQueue)
            {
                logger.LogInformation("Trigger processing is already in progress.");
                return;
            }

            _isProcessingQueue = true;
            logger.LogInformation("Starting trigger queue processing.");
        }

        while (true)
        {
            WorkflowTrigger currentTrigger;
            lock (_triggerQueue)
            {
                if (_triggerQueue.Count == 0)
                {
                    _isProcessingQueue = false;
                    logger.LogInformation("Trigger queue is empty. Stopping processing.");
                    break;
                }
                currentTrigger = _triggerQueue.Dequeue();
                logger.LogInformation("Dequeued trigger: {Trigger}. Remaining queue size: {QueueSize}", currentTrigger, _triggerQueue.Count);
            }
            await ProcessTriggerAsync(currentTrigger);

            if (autoCommit)
            {
                await SaveWorkflowStateAsync();    
                logger.LogInformation("Workflow state saved after processing trigger {Trigger}.", trigger);
            }
        }
    }

    private async Task ProcessTriggerAsync(WorkflowTrigger trigger)
    {
        logger.LogInformation("Processing trigger {Trigger}. Current state: {CurrentState}", trigger, Machine.CurrentState);

        await _fireAsyncLock.WaitAsync();
        try
        {
            logger.LogInformation("Firing trigger {Trigger}...", trigger);
            await Machine.FireAsync(trigger);
            logger.LogInformation("Trigger {Trigger} fired successfully. New state: {NewState}", trigger, Machine.CurrentState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing trigger {Trigger}.", trigger);
            throw;
        }
        finally
        {
            _fireAsyncLock.Release();
            logger.LogInformation("Released lock after processing trigger {Trigger}.", trigger);
        }
    }















    public virtual async Task<bool> ReHydrateAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        return await _asyncLockHelper.ExecuteSynchronizedAsync(async () =>
        {
            WorkflowDto? workflowInstance = await WorkflowService.GetWorkflowInstanceAsync(workflowId);
            if (workflowInstance is null)
            {
                logger.LogWarning("Workflow instance {WorkflowId} was not found.", workflowId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(workflowInstance.InstanceDataJson))
            {
                logger.LogWarning("Workflow instance {WorkflowId} has empty or missing instance data JSON. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                WorkflowContext = new TContext();
            }
            else
            {
                try
                {
                    WorkflowContext = JsonSerializer.Deserialize<TContext>(workflowInstance.InstanceDataJson) ?? new TContext();
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize workflow instance data for WorkflowId {WorkflowId}. Using a new {TContext} instance.", workflowId, typeof(TContext).Name);
                    WorkflowContext = new TContext();
                }
            }

            WorkflowId = workflowId;
            InitializeStateMachine(workflowInstance.Phase);

            return true;
        }, cancellationToken);
    }

    public async Task<WorkflowPhase> TryAlterTaskAsync(IAlterTaskPayload payload)
    {
        if (!CanAlterTaskAsync())
            return WorkflowPhase.Completed;

        return await AlterTaskInternalAsync(payload);
    }

    protected void InitializeStateMachine(WorkflowPhase initialPhase)
    {
        Machine = StateMachineFactory.Create<WorkflowPhase, WorkflowTrigger>(initialPhase);
        ConfigureStateMachine();
    }

    protected virtual void UpdateReplicatorState(Guid taskGuid, bool isCompleted)
    {
        // Default implementation: does nothing
    }

    protected virtual async Task<WorkflowPhase> SaveWorkflowStateAsync()
    {
        await WorkflowService.UpdateWorkflowStateAsync(WorkflowId, Machine.CurrentState, WorkflowContext);
        return Machine.CurrentState;
    }

    protected abstract void OnWorkflowActivated(IWorkflowPayload payload);

    protected abstract void ConfigureStateMachine();

    protected abstract Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload);

    private bool CanAlterTaskAsync()
    {
        if (Machine.CurrentState == WorkflowPhase.Completed)
        {
            logger.LogDebug("Workflow is already completed. No modifications allowed.");
            return false;
        }

        return true;
    }
}