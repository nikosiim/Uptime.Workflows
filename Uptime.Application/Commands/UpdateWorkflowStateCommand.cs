using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;

namespace Uptime.Application.Commands;

public class UpdateWorkflowStateCommand : IRequest
{
    public int WorkflowId { get; set; }
    public WorkflowStatus Status { get; set; }
    public string? InstanceDataJson { get; set; }
}

public class UpdateWorkflowStateCommandHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<UpdateWorkflowStateCommand>
{
    public async Task Handle(UpdateWorkflowStateCommand request, CancellationToken ct)
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == request.WorkflowId, cancellationToken: ct);
        if (instance == null)
        {
            throw new Exception($"Workflow {request.WorkflowId} not found.");
        }

        instance.Status = request.Status;
        instance.InstanceDataJson = request.InstanceDataJson;

        await dbContext.SaveChangesAsync(ct);
    }
}