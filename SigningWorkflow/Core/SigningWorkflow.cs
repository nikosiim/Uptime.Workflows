using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Services;

namespace SigningWorkflow;

public class SigningWorkflow(
    IWorkflowService workflowService, 
    ITaskService taskService, 
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    ILogger<WorkflowBase<BaseWorkflowContext>> logger)
    : ActivityWorkflowBase<BaseWorkflowContext>(workflowService, taskService, historyService, logger)
{
    private readonly ITaskService _taskService = taskService;
    private readonly IHistoryService _historyService = historyService;
    private readonly ILogger<WorkflowBase<BaseWorkflowContext>> _logger = logger;

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

    protected override async Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        await base.OnWorkflowActivatedAsync(cancellationToken);

        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";
    }

    protected override Task PrepareInputDataAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnTaskAlteredAsync(WorkflowEventType action, WorkflowActivityContext activityContext,
        PrincipalSid executorSid, Dictionary<string, string?> inputData, CancellationToken cancellationToken)
    {
        var activity = new SigningTaskActivity(_taskService, _historyService, principalResolver, WorkflowContext);
        
        await activity.ChangedTaskAsync(action, activityContext, executorSid, inputData, cancellationToken);

        IsTaskRejected = activity.IsTaskRejected;

        if (activity.IsCompleted)
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
        List<string> signerSids = WorkflowContext.GetTaskSids();
        if (signerSids.Count < 1)
            throw new WorkflowValidationException(ErrorCode.Validation, "Signer SID not provided.");

        int? dueDays = WorkflowContext.GetTaskDueDays();
        DateTime? dueDate = dueDays.HasValue ? DateTime.UtcNow.AddDays(dueDays.Value) : null;
        
        WorkflowActivityContext activityContext = WorkflowActivityContextFactory.CreateNew(
            phaseId : null,
            assignedToSid: (PrincipalSid)signerSids.First(),
            WorkflowContext.GetTaskDescription(),
            dueDate);
        
        var taskActivity = new SigningTaskActivity(_taskService, _historyService, principalResolver, WorkflowContext);
        await taskActivity.ExecuteAsync(activityContext, cancellationToken);

        _logger.LogSigningTaskCreated(WorkflowDefinition, WorkflowId, AssociationName);
    }

    private void OnSigningTaskCompleted()
    {
        _logger.LogSigningTaskCompleted(WorkflowDefinition, WorkflowId, AssociationName);
    }
}