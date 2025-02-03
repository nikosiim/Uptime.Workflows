using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.Interfaces;
using Uptime.Shared.Enums;

namespace Uptime.Application.Services;

public class TaskService(IMediator mediator) : ITaskService
{
    public async Task<int> CreateWorkflowTaskAsync(IWorkflowTask task)
    {
        var cmd = new CreateUserTaskCommand
        {
            WorkflowId = task.WorkflowId,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            TaskDescription = task.TaskDescription,
            DueDate = task.DueDate,
            Status = WorkflowTaskStatus.NotStarted,
            StorageJson = JsonSerializer.Serialize(task.Storage)
        };
        
        return await mediator.Send(cmd);
    }

    public async Task UpdateWorkflowTaskAsync(IWorkflowTask task, WorkflowTaskStatus taskStatus)
    {
        var cmd = new UpdateUserTaskCommand
        {
            TaskId = task.Id,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            TaskDescription = task.TaskDescription,
            DueDate = task.DueDate,
            Status = taskStatus,
            StorageJson = JsonSerializer.Serialize(task.Storage)
        };

        await mediator.Send(cmd);
    }
}