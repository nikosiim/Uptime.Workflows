using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace SigningWorkflow;

public class SigningWorkflow(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService, 
    IPrincipalResolver principalResolver,
    ILogger<WorkflowBase<SigningWorkflowContext>> logger)
    : ActivityWorkflowBase<SigningWorkflowContext>(workflowService, taskService, historyService, principalResolver, logger)
{
    private readonly ITaskService _taskService = taskService;
    private readonly IHistoryService _historyService = historyService;
    private readonly ILogger<WorkflowBase<SigningWorkflowContext>> _logger = logger;
    private readonly IPrincipalResolver _principalResolver = principalResolver;

    private bool IsTaskRejected { get; set; }

    protected override IWorkflowDefinition WorkflowDefinition => new SigningWorkflowDefinition();
    
    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, ExtendedState.Signing);
        Machine.Configure(ExtendedState.Signing)
            .OnEntryAsync(() => StartSigningTask(cancellationToken))
            .OnExit(OnSigningTaskCompleted)
            .Permit(WorkflowTrigger.TaskCompleted, BaseState.Completed)
            .Permit(WorkflowTrigger.TaskRejected, BaseState.Completed)
            .Permit(WorkflowTrigger.Cancel, BaseState.Cancelled);
        Machine.Configure(BaseState.Completed);
    }

    protected override Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";

        List<string> signerSids = WorkflowContext.GetTaskPrincipalIds();
        if (signerSids.Count < 1)
            throw new WorkflowValidationException(ErrorCode.Validation, "Signer PrincipalId could not be resolved.");

        int? dueDays = WorkflowContext.GetTaskDueDays();

        WorkflowContext.SigningTask = new UserTaskActivityData
        {
            AssignedByPrincipalId = WorkflowContext.GetInitiatorId(),
            AssignedToPrincipalId = PrincipalId.Parse(signerSids.FirstOrDefault()),
            TaskDescription = WorkflowContext.GetTaskDescription(),
            DueDate = dueDays.HasValue ? DateTime.UtcNow.AddDays(dueDays.Value) : null
        };

        return Task.CompletedTask;
    }

    protected override async Task PrepareInputDataAsync(CancellationToken cancellationToken)
    {
        await WorkflowInputPreparerBase.ResolveAndStorePrincipalIdsAsync(
            ctx => ctx.GetTaskSids(),
            (ctx, ids) => ctx.SetTaskPrincipalIds(ids),
            WorkflowContext,
            _principalResolver,
            cancellationToken);
    }

    protected override async Task OnTaskAlteredAsync(AlterTaskPayload payload, CancellationToken cancellationToken)
    {
        var taskActivity = new SigningTaskActivity(_taskService, _historyService, payload.Context)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.ChangedTaskAsync(payload.ExecutedByPrincipalId, payload.InputData, cancellationToken);

        IsTaskRejected = taskActivity.IsTaskRejected;

        if (taskActivity.IsCompleted)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted, cancellationToken);
        }
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = IsTaskRejected ? ExtendedOutcome.Rejected : ExtendedOutcome.Signed;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }

    private async Task StartSigningTask(CancellationToken cancellationToken)
    {
        _logger.LogSigningTaskCreated(WorkflowDefinition, WorkflowId, AssociationName);

        if (WorkflowContext.SigningTask == null)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted, cancellationToken);
            return;
        }

        UserTaskActivityData taskData = WorkflowContext.SigningTask!;

        var workflowTaskContext = new WorkflowTaskContext
        {
            WorkflowId = WorkflowId,
            TaskGuid = Guid.NewGuid()
        };

        var taskActivity = new SigningTaskActivity(_taskService, _historyService, workflowTaskContext)
        {
            TaskData = taskData
        };

        await taskActivity.ExecuteAsync(cancellationToken);
    }

    private void OnSigningTaskCompleted()
    {
        _logger.LogSigningTaskCompleted(WorkflowDefinition, WorkflowId, AssociationName);
    }
}