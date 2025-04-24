using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Uptime.Domain.Common;
using Uptime.Domain.Data;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowRepository(WorkflowDbContext dbContext) : IWorkflowRepository
{
    #region Workflows

    public async Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        var instance = new Workflow
        {
            IsActive = true,
            Phase = BaseState.NotStarted.Value,
            StorageJson = null,
            Originator = payload.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = payload.DocumentId.Value,
            WorkflowTemplateId = payload.WorkflowTemplateId.Value
        };
        
        dbContext.Workflows.Add(instance);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (WorkflowId)instance.Id;
    }

    public async Task MarkWorkflowAsInvalidAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        instance.IsActive = false;
        instance.Outcome = WorkflowOutcome.Invalid.Value;
        instance.Phase = BaseState.Invalid.Value;
        instance.EndDate = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SaveWorkflowStateAsync<TContext>(WorkflowId workflowId, BaseState phase, TContext context, CancellationToken cancellationToken)
        where TContext : IWorkflowContext, new()
    {
        Workflow? instance =  await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        var existingContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
        WorkflowContextHelper.MergeContext(existingContext, context);

        instance.Phase = phase.Value;
        instance.StorageJson = JsonSerializer.Serialize(context);
        instance.Outcome = context.Outcome.Value;

        if (phase.IsFinal())
        {
            instance.EndDate = DateTime.UtcNow;
            instance.IsActive = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region WorkflowTasks

    public async Task<TaskId> CreateWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken)
    {
        bool workflowExists = await dbContext.Workflows.AnyAsync(w => w.Id == request.WorkflowId.Value, cancellationToken: cancellationToken);
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} does not exist.");
        }

        const WorkflowTaskStatus status = WorkflowTaskStatus.NotStarted;
        
        var workflowTask = new WorkflowTask
        {
            WorkflowId = request.WorkflowId.Value,
            TaskGuid = request.TaskGuid,
            AssignedTo = request.AssignedTo,
            AssignedBy = request.AssignedBy,
            Description = request.TaskDescription,
            DueDate = request.DueDate,
            Status = status.ToString(),
            InternalStatus = status,
            PhaseId = request.PhaseId,
            StorageJson = JsonSerializer.Serialize(request.Storage)
        };

        dbContext.WorkflowTasks.Add(workflowTask);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (TaskId)workflowTask.Id;
    }

    public async Task CancelAllActiveTasksAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        List<WorkflowTask> tasks = await dbContext.WorkflowTasks
            .Where(t => t.WorkflowId == workflowId.Value && t.InternalStatus != WorkflowTaskStatus.Completed && t.InternalStatus != WorkflowTaskStatus.Cancelled)
            .ToListAsync(cancellationToken);

        const WorkflowTaskStatus status = WorkflowTaskStatus.Cancelled;

        foreach (WorkflowTask task in tasks)
        {
            task.Status = status.ToString();
            task.InternalStatus = status;
            task.EndDate = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SaveWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken)
    {
        WorkflowTask? task = await dbContext.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId.Value, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");
        }

        WorkflowTaskStatus status = request.TaskStatus;
        
        task.TaskGuid = request.TaskGuid;
        task.AssignedTo = request.AssignedTo;
        task.AssignedBy = request.AssignedBy;
        task.Description = request.TaskDescription;
        task.DueDate = request.DueDate;
        task.Status = status.ToString();
        task.InternalStatus = status;
        task.StorageJson = JsonSerializer.Serialize(request.Storage);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region WorkflowHistory

    public async Task AddWorkflowHistoryAsync(
        WorkflowId workflowId,
        WorkflowEventType eventType,
        string? author,
        string? description,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var historyEntry = new WorkflowHistory
        {
            Event = eventType,
            User = author,
            Occurred = DateTime.UtcNow,
            Description = description,
            Comment = comment,
            WorkflowId = workflowId.Value
        };
        
        dbContext.WorkflowHistories.Add(historyEntry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}