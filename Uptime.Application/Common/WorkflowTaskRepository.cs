using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public class WorkflowTaskRepository(IWorkflowDbContext dbContext) : IWorkflowTaskRepository
{
    public async Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask request)
    {
        bool workflowExists = await dbContext.Workflows.AnyAsync(w => w.Id == request.WorkflowId.Value);
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} does not exist.");
        }

        var task = new WorkflowTask
        {
            WorkflowId = request.WorkflowId.Value,
            TaskGuid = request.TaskGuid,
            AssignedTo = request.AssignedTo,
            AssignedBy = request.AssignedBy,
            Description = request.TaskDescription,
            DueDate = request.DueDate,
            Status = WorkflowTaskStatus.NotStarted,
            StorageJson = JsonSerializer.Serialize(request.Storage)
        };

        dbContext.WorkflowTasks.Add(task);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        return (TaskId)task.Id;
    }

    public async Task<WorkflowTaskContext?> GetWorkflowTaskContextAsync(TaskId taskId)
    {
        WorkflowTask? workflowTask = await dbContext.WorkflowTasks.Where(task => task.Id == taskId.Value).FirstOrDefaultAsync();
        if (workflowTask is null)
            return null;

        return new WorkflowTaskContext((WorkflowId)workflowTask.WorkflowId)
        {
            TaskId = taskId,
            TaskGuid = workflowTask.TaskGuid,
            AssignedTo = workflowTask.AssignedTo,
            AssignedBy = workflowTask.AssignedBy,
            TaskDescription = workflowTask.Description,
            DueDate = workflowTask.DueDate,
            Storage = JsonSerializer.Deserialize<Dictionary<string, string?>>(workflowTask.StorageJson ?? "{}") ?? new Dictionary<string, string?>()
        };
    }

    public async Task SaveWorkflowTaskAsync(IWorkflowTask request, WorkflowTaskStatus taskStatus)
    {
        WorkflowTask? task = dbContext.WorkflowTasks.FirstOrDefault(t => t.Id == request.TaskId.Value);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");
        }

        task.TaskGuid = request.TaskGuid;
        task.AssignedTo = request.AssignedTo;
        task.AssignedBy = request.AssignedBy;
        task.Description = request.TaskDescription;
        task.DueDate = request.DueDate;
        task.Status = taskStatus;
        task.StorageJson = JsonSerializer.Serialize(request.Storage);

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}