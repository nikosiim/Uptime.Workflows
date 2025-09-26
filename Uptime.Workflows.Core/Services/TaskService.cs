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
    public async Task<TaskId> CreateAsync(WorkflowId workflowId, IWorkflowActivityContext activityContext, CancellationToken cancellationToken)
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
            TaskGuid = activityContext.TaskGuid,
            AssignedToPrincipalId = activityContext.AssignedToPrincipalId.Value,
            AssignedByPrincipalId = activityContext.AssignedByPrincipalId.Value,
            DueDate = activityContext.DueDate,
            Description = activityContext.Description,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = activityContext.PhaseId,
            StorageJson = JsonSerializer.Serialize(activityContext.Storage)
        };

        db.WorkflowTasks.Add(workflowTask);
        await db.SaveChangesAsync(cancellationToken);

        return (TaskId)workflowTask.Id;
    }
    
    public async Task UpdateAsync(IWorkflowActivityContext activityContext, CancellationToken cancellationToken)
    {
        int taskId = activityContext.GetTaskId().Value;
        if (taskId <= 0)
        {
            throw new ArgumentException("Invalid TaskId value on activity context before UpdateAsync.");
        }

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        WorkflowTask? task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }

        WorkflowTaskStatus status = activityContext.GetTaskStatus();

        task.TaskGuid = activityContext.TaskGuid;
        task.DueDate = activityContext.DueDate;
        task.Description = activityContext.Description;
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.AssignedToPrincipalId = activityContext.AssignedToPrincipalId.Value;
        task.AssignedByPrincipalId = activityContext.AssignedByPrincipalId.Value;
        task.StorageJson = JsonSerializer.Serialize(activityContext.Storage);

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