using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record CreateUserTaskCommand : IRequest<TaskId>
{
    public WorkflowId WorkflowId { get; init; }
    public required string AssignedTo { get; init; }
    public required string AssignedBy { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate  { get; init; }
    public string? StorageJson  { get; init; }
    public WorkflowTaskStatus Status { get; init; }
}

public class CreateUserTaskCommandHandler(IWorkflowDbContext context) : IRequestHandler<CreateUserTaskCommand, TaskId>
{
    public async Task<TaskId> Handle(CreateUserTaskCommand request, CancellationToken cancellationToken)
    {
        bool workflowExists = await context.Workflows.AnyAsync(w => w.Id == request.WorkflowId.Value, cancellationToken);
    
        if (!workflowExists)
        {
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} does not exist.");
        }

        var task = new WorkflowTask
        {
            WorkflowId = request.WorkflowId.Value,
            AssignedTo = request.AssignedTo,
            AssignedBy = request.AssignedBy,
            TaskDescription = request.TaskDescription,
            DueDate = request.DueDate,
            Status = request.Status,
            StorageJson = request.StorageJson
        };

        context.WorkflowTasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        return (TaskId)task.Id;
    }
}