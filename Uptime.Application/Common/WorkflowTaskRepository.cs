using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public class WorkflowTaskRepository(IWorkflowDbContext dbContext) : IWorkflowTaskRepository
{
    public async Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken)
    {
        bool workflowExists = await dbContext.Workflows.AnyAsync(w => w.Id == request.WorkflowId.Value, cancellationToken: cancellationToken);
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
        await dbContext.SaveChangesAsync(cancellationToken);

        return (TaskId)task.Id;
    }
    
    public async Task SaveWorkflowTaskAsync(IWorkflowTask request, WorkflowTaskStatus taskStatus, CancellationToken cancellationToken)
    {
        WorkflowTask? task = await dbContext.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId.Value, cancellationToken);

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

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}