using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Domain.Enums;
using Uptime.Shared;
using Uptime.Shared.Extensions;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflow(IWorkflowService workflowService, ITaskService taskService)
    : WorkflowBase<SigningWorkflowContext>(workflowService)
{
    protected override void ConfigureStateMachine()
    {
        Machine.Configure(WorkflowPhase.NotStarted)
            .Permit(WorkflowTrigger.Start, WorkflowPhase.Signing);

        Machine.Configure(WorkflowPhase.Signing)
            .OnEntryAsync(StartSigningTask)
            .Permit(WorkflowTrigger.TaskCompleted, WorkflowPhase.Completed)
            .Permit(WorkflowTrigger.TaskRejected, WorkflowPhase.Rejected);

        Machine.Configure(WorkflowPhase.Completed)
            .OnEntry(() => Console.WriteLine("Signing workflow completed successfully."));

        Machine.Configure(WorkflowPhase.Rejected)
            .OnEntry(() => Console.WriteLine("Signing workflow was rejected."));
    }

    protected override void OnWorkflowActivated(IWorkflowPayload payload)
    {
        if (payload.Storage.TryGetValue(GlobalConstants.TaskStorageKeys.TaskSigners, out string? signer) && !string.IsNullOrWhiteSpace(signer))
        {
            string? taskDescription = payload.Storage.GetValueAsString(GlobalConstants.TaskStorageKeys.SignerTask);
            DateTime dueDate = payload.Storage.GetValueAsDateTime(GlobalConstants.TaskStorageKeys.TaskDueDate);

            WorkflowContext.SigningTask = new SigningTaskData
            {
                AssignedBy = payload.Originator,
                AssignedTo = signer,
                TaskDescription = taskDescription,
                DueDate = dueDate
            };  
        }             
    }

    protected override async Task<WorkflowPhase> AlterTaskInternalAsync(IAlterTaskPayload payload)
    {
        WorkflowTaskContext? context = await taskService.GetWorkflowTaskContextAsync(payload.TaskId);
        if (context == null) 
            return Machine.State;

        var taskActivity = new SigningTaskActivity(taskService, context)
        {
            TaskData = WorkflowContext.SigningTask
        };

        await taskActivity.OnTaskChanged(payload);

        if (taskActivity.IsCompleted)
        {
            await FireAsync(WorkflowTrigger.TaskCompleted);
        }

        return Machine.State;
    }

    private async Task StartSigningTask()
    {
        if (WorkflowContext.SigningTask == null)
        {
            Console.WriteLine("No signing task available, skipping to TaskCompleted.");
            await FireAsync(WorkflowTrigger.TaskCompleted); // TODO: complete task but workflow outcome should be invalid or something
            return;
        }

        SigningTaskData taskData = WorkflowContext.SigningTask!;
        var taskActivity = new SigningTaskActivity(taskService, new WorkflowTaskContext(WorkflowId))
        {
            TaskData = taskData
        };

        await taskActivity.ExecuteAsync();
        await OnSigningTaskCompleted();
    }

    private static Task OnSigningTaskCompleted()
    {
        // TODO: add some logic that need to accomplish after task creation
       return Task.CompletedTask;
    }
}