using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Domain.Workflows;
using Uptime.Shared;
using Uptime.Shared.Extensions;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository,
    ILogger<WorkflowBase<SigningWorkflowContext>> logger)
    : ActivityWorkflowBase<SigningWorkflowContext>(stateMachineFactory, repository, logger)
{
    private readonly IWorkflowRepository _repository = repository;

    protected override void ConfigureStateMachineAsync(CancellationToken cancellationToken)
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Signing);

        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(() => StartSigningTask(cancellationToken))
            .OnExit(OnSigningTaskCompleted) 
            .Permit(WorkflowTrigger.TaskCompleted, WorkflowPhase.Completed)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Rejected);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Signing workflow completed successfully."));

        Machine.Configure(WorkflowPhase.Rejected)
            .OnEntry(() => Console.WriteLine("Signing workflow was rejected."))
            .OnExit(OnWorkflowCompletion);
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.TaskSigners, out string? signer) && !string.IsNullOrWhiteSpace(signer))
        {
            string? taskDescription = payload.Storage.GetValueAsString(GlobalConstants.TaskStorageKeys.TaskDescription);
            string? dueDays = payload.Storage.GetValueAsString(GlobalConstants.TaskStorageKeys.TaskDueDays);

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

    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext context, CancellationToken cancellationToken)
    {
        var taskActivity = new SigningTaskActivity(_repository, context)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.OnTaskChangedAsync(context.Storage, cancellationToken);

        if (taskActivity.IsCompleted)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted, cancellationToken);
        }
    }

    private async Task StartSigningTask(CancellationToken cancellationToken)
    {
        if (WorkflowContext.SigningTask == null)
        {
            Console.WriteLine("No signing task available, skipping to TaskCompleted.");
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

    private static void OnWorkflowCompletion()
    {
       
    }
}