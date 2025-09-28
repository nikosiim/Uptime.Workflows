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
    public async Task CreateAsync(WorkflowId workflowId, PrincipalId assignedBy, PrincipalId assignedTo, IWorkflowActivityContext context, CancellationToken ct)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        bool workflowExists = await db.Workflows.AnyAsync(w => w.Id == workflowId.Value, cancellationToken: ct);
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} does not exist.");
        }

        const WorkflowTaskStatus status = WorkflowTaskStatus.NotStarted;

        var workflowTask = new WorkflowTask
        {
            WorkflowId = workflowId.Value,
            AssignedToPrincipalId = assignedTo.Value,
            AssignedByPrincipalId = assignedBy.Value,
            DueDate = context.DueDate,
            Description = context.Description,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = context.PhaseId,
            StorageJson = JsonSerializer.Serialize(context.Storage)
        };

        workflowTask.SetTaskGuid(context.TaskGuid);

        db.WorkflowTasks.Add(workflowTask);
        await db.SaveChangesAsync(ct);
    }
    
    public async Task UpdateAsync(IWorkflowActivityContext context, CancellationToken ct)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        WorkflowTask? task = await db.WorkflowTasks.FirstOrDefaultAsync(t => t.TaskGuid == context.TaskGuid, ct);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task {context.TaskGuid} not found.");
        }

        WorkflowTaskStatus status = context.GetTaskStatus();
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.StorageJson = JsonSerializer.Serialize(context.Storage);

        await db.SaveChangesAsync(ct);
    }

    public async Task CancelActiveTasksAsync(WorkflowId workflowId, CancellationToken ct)
    {
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        List<WorkflowTask> tasks = await db.WorkflowTasks
            .Where(t => t.WorkflowId == workflowId.Value && t.InternalStatus != WorkflowTaskStatus.Completed && t.InternalStatus != WorkflowTaskStatus.Cancelled)
            .ToListAsync(ct);

        const WorkflowTaskStatus status = WorkflowTaskStatus.Cancelled;

        foreach (WorkflowTask task in tasks)
        {
            task.Status = status.ToString();
            task.InternalStatus = status;
            task.EndDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}