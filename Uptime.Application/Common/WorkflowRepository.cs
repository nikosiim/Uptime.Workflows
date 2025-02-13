using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Enums;

namespace Uptime.Application.Common;

public class WorkflowRepository(IWorkflowDbContext dbContext) : IWorkflowRepository
{
    #region Workflows table

    public async Task<WorkflowId> CreateWorkflowInstanceAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        var instance = new Workflow
        {
            Phase = WorkflowPhase.NotStarted,
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

    public async Task<Workflow?> GetWorkflowInstanceAsync(WorkflowId workflowId, CancellationToken cancellationToken)
    {
        return await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
    }
    
    public async Task SaveWorkflowStateAsync<TContext>(WorkflowId workflowId, WorkflowPhase phase, TContext context, CancellationToken cancellationToken)
        where TContext : IWorkflowContext, new()
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == workflowId.Value, cancellationToken);
        if (instance == null)
        {
            throw new InvalidOperationException($"Workflow with ID {workflowId} not found.");
        }

        var existingContext = WorkflowContextHelper.Deserialize<TContext>(instance.StorageJson);
        WorkflowContextHelper.MergeContext(existingContext, context);

        instance.Phase = phase;
        instance.StorageJson = WorkflowContextHelper.Serialize(context);

        // Set the end date if the workflow is in a final state.
        if (phase == WorkflowPhase.Completed ||
            phase == WorkflowPhase.Cancelled ||
            phase == WorkflowPhase.Terminated)
        {
            instance.EndDate = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region WorkflowTasks table

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
    
    public async Task SaveWorkflowTaskAsync(IWorkflowTask request, CancellationToken cancellationToken)
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
        task.Status = request.TaskStatus;
        task.StorageJson = JsonSerializer.Serialize(request.Storage);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}