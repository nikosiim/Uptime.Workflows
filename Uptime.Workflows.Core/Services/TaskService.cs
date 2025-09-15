using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Services;

public class TaskService(IDbContextFactory<WorkflowDbContext> factory) : ITaskService
{
    public async Task<TaskId> CreateAsync(WorkflowId workflowId, IWorkflowTaskContext taskContext, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        bool workflowExists = await db.Workflows.AnyAsync(w => w.Id == workflowId.Value, cancellationToken: cancellationToken);
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} does not exist.");
        }

        const WorkflowTaskStatus status = WorkflowTaskStatus.NotStarted;

        var workflowTask = new WorkflowTask
        {
            WorkflowId = workflowId.Value,
            TaskGuid = taskContext.TaskGuid,
            AssignedToPrincipalId = taskContext.AssignedToPrincipalId.Value,
            AssignedByPrincipalId = taskContext.AssignedByPrincipalId.Value,
            DueDate = taskContext.DueDate,
            Description = taskContext.Description,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = taskContext.PhaseId,
            StorageJson = JsonSerializer.Serialize(taskContext.Storage)
        };

        db.WorkflowTasks.Add(workflowTask);
        await db.SaveChangesAsync(cancellationToken);

        return (TaskId)workflowTask.Id;
    }
    
    public async Task UpdateAsync(IWorkflowTaskContext taskContext, CancellationToken cancellationToken)
    {
        int taskId = taskContext.GetTaskId().Value;

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        WorkflowTask? task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }

        WorkflowTaskStatus status = taskContext.GetTaskStatus();

        task.TaskGuid = taskContext.TaskGuid;
        task.DueDate = taskContext.DueDate;
        task.Description = taskContext.Description;
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.AssignedToPrincipalId = taskContext.AssignedToPrincipalId.Value;
        task.AssignedByPrincipalId = taskContext.AssignedByPrincipalId.Value;
        task.StorageJson = JsonSerializer.Serialize(taskContext.Storage);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        List<WorkflowTask> tasks = await db.WorkflowTasks
            .Where(t => t.WorkflowId == workflowId.Value && t.InternalStatus != WorkflowTaskStatus.Completed && t.InternalStatus != WorkflowTaskStatus.Cancelled)
            .ToListAsync(cancellationToken);

        const WorkflowTaskStatus status = WorkflowTaskStatus.Cancelled;

        foreach (WorkflowTask task in tasks)
        {
            task.Status = status.ToString();
            task.InternalStatus = status;
            task.EndDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}