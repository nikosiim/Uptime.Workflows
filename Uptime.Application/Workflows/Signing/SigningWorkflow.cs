﻿using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;
using Uptime.Shared;
using Uptime.Shared.Extensions;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflow(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<SigningWorkflowContext>> logger)
    : ActivityWorkflowBase<SigningWorkflowContext>(stateMachineFactory, repository, logger)
{
    private readonly IWorkflowRepository _repository = repository;

    protected string? AssociationName => WorkflowContext.Storage.GetValueOrDefault(GlobalConstants.WorkflowStorageKeys.AssociationName);

    protected override IWorkflowDefinition WorkflowDefinition => throw new NotImplementedException();

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

        if (payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.TaskSigners, out string? signer) && !string.IsNullOrWhiteSpace(signer))
        {
            string? taskDescription = payload.Storage.GetValue(GlobalConstants.TaskStorageKeys.TaskDescription);
            string? dueDays = payload.Storage.GetValue(GlobalConstants.TaskStorageKeys.TaskDueDays);

            int.TryParse(dueDays, out int days);

            WorkflowContext.SigningTask = new SigningTaskData
            {
                AssignedBy = payload.Originator,
                AssignedTo = signer,
                TaskDescription = taskDescription,
                DueDate = DateTime.Now.AddDays(days)
            };
        }             
    }

    protected override async Task OnTaskChangedAsync(WorkflowTaskContext taskContext, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        var taskActivity = new SigningTaskActivity(_repository, taskContext)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.ChangedTaskAsync(payload, cancellationToken);

        if (taskActivity.IsCompleted)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted, cancellationToken);
        }
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        WorkflowContext.Outcome = ExtendedOutcome.Signed;
        WorkflowCompletedHistoryDescription = $"{AssociationName} on lõpetatud.";

        return Task.CompletedTask;
    }

    private async Task StartSigningTask(CancellationToken cancellationToken)
    {
        if (WorkflowContext.SigningTask == null)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted, cancellationToken); // TODO: complete task but workflow outcome should be invalid or something
            return;
        }

        SigningTaskData taskData = WorkflowContext.SigningTask!;
        var taskActivity = new SigningTaskActivity(_repository, new WorkflowTaskContext(WorkflowId))
        {
            TaskData = taskData
        };

        await taskActivity.ExecuteAsync(cancellationToken);
    }
    
    private static void OnSigningTaskCompleted()
    {
  
    }
}