﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;

namespace Uptime.Application.Commands;

public class UpdateUserTaskCommand : IRequest
{
    public int TaskId { get; set; }
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
        WorkflowTask? task = await context.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);
        
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");
        }

        task.AssignedTo = request.AssignedTo;
        task.AssignedBy = request.AssignedBy;
        task.TaskDescription = request.TaskDescription;
        task.DueDate = request.DueDate;
        task.StorageJson = request.StorageJson;
        task.Status = request.Status;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}