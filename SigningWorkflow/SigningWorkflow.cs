using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;
using static SigningWorkflow.Constants;

namespace SigningWorkflow;

public class SigningWorkflow(IWorkflowService workflowService, ITaskService taskService, IHistoryService historyService, IPrincipalResolver principalResolver,
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

    protected override async Task OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        WorkflowStartedHistoryDescription = $"{AssociationName} on alustatud.";

        if (!payload.Storage.TryGetValue(TaskStorageKeys.TaskSignerSid, out string? signerSid) || string.IsNullOrWhiteSpace(signerSid))
        {
            throw new WorkflowValidationException(ErrorCode.Validation, "Signer SID is missing in workflow start payload.");
        }

        string? taskDescription = payload.Storage.GetValue(TaskStorageKeys.TaskDescription);
        string? dueDaysText = payload.Storage.GetValue(TaskStorageKeys.TaskDueDays);
        _ = int.TryParse(dueDaysText, out int days);

        Principal? signer = await principalResolver.ResolveBySidAsync(signerSid, cancellationToken);
        if (signer is null)
        {
            throw new WorkflowValidationException(ErrorCode.NotFound, $"Signer not found for SID '{signerSid}'.");
        }

        WorkflowContext.SigningTask = new UserTaskActivityData
        {
            AssignedByPrincipalId = payload.InitiatedByPrincipalId,
            AssignedToPrincipalId = signer.Id,
            TaskDescription = taskDescription,
            DueDate = days > 0 ? DateTime.UtcNow.AddDays(days) : null
        };
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