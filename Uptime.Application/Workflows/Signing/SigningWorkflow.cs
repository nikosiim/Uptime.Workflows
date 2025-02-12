using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Shared;
using Uptime.Shared.Extensions;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflow(
    IStateMachineFactory<WorkflowPhase, WorkflowTrigger> stateMachineFactory,
    IWorkflowStateRepository<SigningWorkflowContext> stateRepository,
    IWorkflowTaskRepository taskService, 
    ILogger<WorkflowBase<SigningWorkflowContext>> logger)
    : ActivityWorkflowBase<SigningWorkflowContext>(stateMachineFactory, stateRepository, logger)
{
    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Signing);

        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(StartSigningTask)
            .OnExitAsync(OnSigningTaskCompleted) 
            .Permit(WorkflowTrigger.TaskCompleted, WorkflowPhase.Completed)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Rejected);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Signing workflow completed successfully."));

        Machine.Configure(WorkflowPhase.Rejected)
            .OnEntry(() => Console.WriteLine("Signing workflow was rejected."))
            .OnExitAsync(OnWorkflowCompletion);
    }

    protected override void OnWorkflowActivated(IWorkflowPayload payload)
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

    protected override async Task AlterTaskInternalAsync(WorkflowTaskContext context)
    {
        var taskActivity = new SigningTaskActivity(taskService, context)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.OnTaskChanged(context.Storage);

        if (taskActivity.IsCompleted)
        {
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted);
        }
    }

    private async Task StartSigningTask()
    {
        if (WorkflowContext.SigningTask == null)
        {
            Console.WriteLine("No signing task available, skipping to TaskCompleted.");
            await TriggerTransitionAsync(WorkflowTrigger.TaskCompleted); // TODO: complete task but workflow outcome should be invalid or something
            return;
        }

        SigningTaskData taskData = WorkflowContext.SigningTask!;
        var taskActivity = new SigningTaskActivity(taskService, new WorkflowTaskContext(WorkflowId))
        {
            TaskData = taskData
        };

        await taskActivity.ExecuteAsync();
    }

    private static Task OnSigningTaskCompleted()
    {
        // TODO: add some logic that need to accomplish after task creation
       return Task.CompletedTask;
    }

    private static Task OnWorkflowCompletion()
    {
        return Task.CompletedTask;
    }
}