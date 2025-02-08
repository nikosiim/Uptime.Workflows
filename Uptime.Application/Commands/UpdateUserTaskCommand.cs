using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record UpdateUserTaskCommand : IRequest
{
    public TaskId TaskId { get; init; }
    public required Guid TaskGuid { get; init; }
    public required string AssignedTo { get; init; }
    public required string AssignedBy { get; init; }
    public string? TaskDescription { get; init; }
    public DateTime? DueDate  { get; init; }
    public string? StorageJson  { get; init; }
    public WorkflowTaskStatus Status { get; init; }
}

public class UpdateUserTaskCommandHandler(IWorkflowDbContext context) : IRequestHandler<UpdateUserTaskCommand>
{
    public async Task Handle(UpdateUserTaskCommand request, CancellationToken cancellationToken)
    {
        WorkflowTask? task = await context.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId.Value, cancellationToken);
        
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");
        }

        task.TaskGuid = request.TaskGuid;
        task.AssignedTo = request.AssignedTo;
        task.AssignedBy = request.AssignedBy;
        task.Description = request.TaskDescription;
        task.DueDate = request.DueDate;
        task.StorageJson = request.StorageJson;
        task.Status = request.Status;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}