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
    public async Task CreateAsync(WorkflowId workflowId, IWorkflowActivityContext activityContext, CancellationToken cancellationToken)
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
            AssignedToPrincipalId = activityContext.AssignedToPrincipalId.Value,
            AssignedByPrincipalId = activityContext.AssignedByPrincipalId.Value,
            DueDate = activityContext.DueDate,
            Description = activityContext.Description,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = activityContext.PhaseId,
            StorageJson = JsonSerializer.Serialize(activityContext.Storage)
        };

        workflowTask.SetTaskGuid(activityContext.TaskGuid);

        db.WorkflowTasks.Add(workflowTask);
        await db.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateAsync(IWorkflowActivityContext context, CancellationToken cancellationToken)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        WorkflowTask? task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.TaskGuid == context.TaskGuid, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task {context.TaskGuid} not found.");
        }

        WorkflowTaskStatus status = context.GetTaskStatus();
        task.DueDate = context.DueDate;
        task.Description = context.Description;
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.AssignedToPrincipalId = context.AssignedToPrincipalId.Value;
        task.AssignedByPrincipalId = context.AssignedByPrincipalId.Value;
        task.StorageJson = JsonSerializer.Serialize(context.Storage);

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