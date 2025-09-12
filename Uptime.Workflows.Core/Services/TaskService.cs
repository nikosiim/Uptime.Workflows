using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Services;

public class TaskService(IDbContextFactory<WorkflowDbContext> factory) : ITaskService
{
    public async Task<TaskId> CreateAsync(IWorkflowTask request, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        bool workflowExists = await db.Workflows.AnyAsync(w => w.Id == request.WorkflowId.Value, cancellationToken: cancellationToken);
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} does not exist.");
        }

        const WorkflowTaskStatus status = WorkflowTaskStatus.NotStarted;

        var workflowTask = new WorkflowTask
        {
            WorkflowId = request.WorkflowId.Value,
            TaskGuid = request.TaskGuid,
            AssignedToPrincipalId = request.AssignedToPrincipalId.Value,
            AssignedByPrincipalId = request.AssignedByPrincipalId.Value,
            Description = request.TaskDescription,
            DueDate = request.DueDate,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = request.PhaseId,
            StorageJson = JsonSerializer.Serialize(request.Storage)
        };

        db.WorkflowTasks.Add(workflowTask);
        await db.SaveChangesAsync(cancellationToken);

        return (TaskId)workflowTask.Id;
    }
    
    public async Task UpdateAsync(IWorkflowTask request, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        WorkflowTask? task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId.Value, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");
        }

        WorkflowTaskStatus status = request.TaskStatus;

        task.TaskGuid = request.TaskGuid;
        task.AssignedToPrincipalId = request.AssignedToPrincipalId.Value;
        task.AssignedByPrincipalId = request.AssignedByPrincipalId.Value;
        task.Description = request.TaskDescription;
        task.DueDate = request.DueDate;
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.StorageJson = JsonSerializer.Serialize(request.Storage);

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