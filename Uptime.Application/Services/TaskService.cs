using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Services;

public class TaskService(IMediator mediator) : ITaskService
{
    public async Task<WorkflowTaskContext?> GetWorkflowTaskContextAsync(TaskId taskId)
    {
        WorkflowTaskDto? dto = await mediator.Send(new GetWorkflowTaskQuery(taskId));
        if (dto is null)
            return null;

        return new WorkflowTaskContext((WorkflowId)dto.WorkflowId)
        {
            TaskId = taskId,
            AssignedTo = dto.AssignedTo ?? string.Empty,
            AssignedBy = dto.AssignedBy ?? string.Empty,
            TaskDescription = dto.Description,
            DueDate = dto.DueDate,
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(dto.StorageJson ?? "{}") ?? new Dictionary<string, string?>()
        };
    }


    public async Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask task)
    {
        var cmd = new CreateUserTaskCommand
        {
            WorkflowId = task.WorkflowId,
            TaskGuid = task.TaskGuid,
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
            TaskGuid = task.TaskGuid,
            TaskId = task.TaskId,
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