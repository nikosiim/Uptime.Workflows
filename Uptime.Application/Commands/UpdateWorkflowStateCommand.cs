using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;

namespace Uptime.Application.Commands;

public class UpdateWorkflowStateCommand : IRequest
{
    public WorkflowId WorkflowId { get; set; }
    public WorkflowPhase Phase { get; set; }
    public DateTime? EndDate { get; set; }
    public string? StorageJson { get; set; }
}

public class UpdateWorkflowStateCommandHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<UpdateWorkflowStateCommand>
{
    public async Task Handle(UpdateWorkflowStateCommand request, CancellationToken ct)
    {
        Workflow? instance = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == request.WorkflowId.Value, cancellationToken: ct);
        if (instance == null)
        {
            throw new Exception($"Workflow {request.WorkflowId} not found.");
        }

        instance.Phase = request.Phase;
        instance.EndDate = request.EndDate;
        instance.StorageJson = request.StorageJson;

        await dbContext.SaveChangesAsync(ct);
    }
}