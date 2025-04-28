using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;
using static SigningWorkflow.Constants;

namespace SigningWorkflow;

public class SigningWorkflow(IWorkflowService workflowService, ITaskService taskService, IHistoryService historyService, 
    ILogger<WorkflowBase<SigningWorkflowContext>> logger)
    : ActivityWorkflowBase<SigningWorkflowContext>(workflowService, taskService, historyService, logger)
{
    private readonly ITaskService _taskService = taskService;
    private readonly IHistoryService _historyService = historyService;
    private readonly ILogger<WorkflowBase<SigningWorkflowContext>> _logger = logger;

    public bool IsTaskRejected { get; private set; }
    
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

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";

        if (payload.Storage.TryGetValue(TaskStorageKeys.TaskSigners, out string? signer) && !string.IsNullOrWhiteSpace(signer))
        {
            string? taskDescription = payload.Storage.GetValue(TaskStorageKeys.TaskDescription);
            string? dueDays = payload.Storage.GetValue(TaskStorageKeys.TaskDueDays);

            _ = int.TryParse(dueDays, out int days);

            WorkflowContext.SigningTask = new UserTaskActivityData
            {
                AssignedBy = payload.Originator,
                AssignedTo = signer,
                TaskDescription = taskDescription,
                DueDate = DateTime.Now.AddDays(days)
            };
        }
    }

    protected override async Task OnTaskAlteredAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        var taskActivity = new SigningTaskActivity(_taskService, _historyService, context)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.ChangedTaskAsync(payload, cancellationToken);

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
        var taskActivity = new SigningTaskActivity(_taskService, _historyService, new WorkflowTaskContext(WorkflowId, Guid.NewGuid()))
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