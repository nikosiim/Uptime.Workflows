using Microsoft.Extensions.Logging;
using System.Text.Json;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace ApprovalWorkflow;

public class ApprovalWorkflow(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IApprovalWorkflowInputPreparer inputPreparer,
    ILogger<WorkflowBase<ApprovalWorkflowContext>> logger)
    : ReplicatorActivityWorkflowBase<ApprovalWorkflowContext>(workflowService, taskService, historyService, principalResolver, logger)
{
    private readonly ITaskService _taskService = taskService;
    private readonly IHistoryService _historyService = historyService;
    
    protected override IWorkflowDefinition WorkflowDefinition => new ApprovalWorkflowDefinition();

    protected override IReplicatorActivityProvider ActivityProvider => new ApprovalWorkflowActivityProvider(_taskService, _historyService);
    
    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, BaseState.InProgress);
        Machine.Configure(BaseState.InProgress)
            .InitialTransition(ExtendedState.Approval)
            .Permit(WorkflowTrigger.Cancel, BaseState.Cancelled)
            .Permit(WorkflowTrigger.TaskRejected, BaseState.Completed);
        Machine.Configure(ExtendedState.Approval)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Approval.Value, cancellationToken))
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, WorkflowPhase.Completed, () => WorkflowContext.AnyTaskRejected)
            //.PermitIf(WorkflowTrigger.AllTasksCompleted, ApprovalPhase.Signing, () => !WorkflowContext.AnyTaskRejected)
            .PermitDynamic(WorkflowTrigger.AllTasksCompleted, () => WorkflowContext.AnyTaskRejected ? BaseState.Completed : ExtendedState.Signing);
        Machine.Configure(ExtendedState.Signing)
            .SubstateOf(BaseState.InProgress)
            .OnEntryAsync(() => RunReplicatorAsync(ExtendedState.Signing.Value, cancellationToken))
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }
    
    protected override async Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        await base.OnWorkflowActivatedAsync(cancellationToken);
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }
    
    protected override Task PrepareInputDataAsync(CancellationToken cancellationToken)
    {
        return inputPreparer.PrepareAsync(WorkflowContext.Storage, cancellationToken);
    }

    protected override IReplicatorPhaseBuilder CreateReplicatorPhaseBuilder()
    {
        var phases = new Dictionary<string, ReplicatorPhaseConfiguration>
        {
            [ExtendedState.Approval.Value] = new()
            {
                ActivityData = workflowContext => workflowContext.GetApprovalTasks(),
                ReplicatorType = workflowContext => workflowContext.GetReplicatorType(ExtendedState.Approval.Value)
            },
            [ExtendedState.Signing.Value] = new()
            {
                ActivityData = workflowContext => workflowContext.GetSigningTasks(),
                ReplicatorType = workflowContext => workflowContext.GetReplicatorType(ExtendedState.Signing.Value)
            }
        };

        return new ReplicatorPhaseBuilder(phases);
    }
    
    protected override string OnWorkflowModification()
    {
        base.OnWorkflowModification();

        ReplicatorState replicatorState = WorkflowContext.ReplicatorStates[Machine.State.Value];

        List<ReplicatorItem> activeItems = replicatorState.Items
            .Where(i => i.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
            .ToList();
        
        var context = new ApprovalModificationContext
        {
            ApprovalTasks = activeItems.Select(activeItem 
                => new ApprovalTask
                {
                    AssignedToPrincipalId = activeItem.Data.DeserializeTaskData<ApprovalTaskData>().AssignedToPrincipalId,
                    TaskGuid = activeItem.TaskGuid.ToString()
                }).ToList()
        };

        return JsonSerializer.Serialize(context);
    }

    protected override Task<bool> OnWorkflowModifiedAsync(ModificationPayload payload, CancellationToken cancellationToken)
    {
        if (!WorkflowContext.ReplicatorStates.TryGetValue(Machine.State.Value, out ReplicatorState? replicatorState))
            return Task.FromResult(false);
        
        ReplicatorItem? inProgressItem = replicatorState.Items.FirstOrDefault(item => item.Status == ReplicatorItemStatus.InProgress);
        if (inProgressItem == null)
            return Task.FromResult(false);

        if (string.IsNullOrWhiteSpace(payload.ModificationContext))
            return Task.FromResult(false);

        var context = JsonSerializer.Deserialize<ApprovalModificationContext>(payload.ModificationContext);
        if (context == null) 
            return Task.FromResult(true);
       
        ApprovalTaskData currentTaskData = inProgressItem.Data.DeserializeTaskData<ApprovalTaskData>();

        replicatorState.Items.RemoveAll(item => item.Status == ReplicatorItemStatus.NotStarted);
        
        foreach (ApprovalTask task in context.ApprovalTasks)
        {
            if (task.AssignedToPrincipalId == currentTaskData.AssignedToPrincipalId) continue;
            
            ApprovalTaskData data = ApprovalTaskData.Copy(currentTaskData, task.AssignedToPrincipalId);

            Guid taskGuid = Guid.Parse(task.TaskGuid);
            if (taskGuid.Equals(Guid.Empty))
            {
                taskGuid = Guid.NewGuid();
            }

            replicatorState.Items.Add(new ReplicatorItem(taskGuid, data));
        }

        return Task.FromResult(true);
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = WorkflowContext.AnyTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Approved;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }
}