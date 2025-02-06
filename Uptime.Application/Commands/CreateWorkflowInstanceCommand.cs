using MediatR;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record CreateWorkflowInstanceCommand : IRequest<int>
{
    public int WorkflowTemplateId { get; init; }
    public int DocumentId { get; init; }
    public string? Originator { get; init; }
}

public class CreateWorkflowInstanceCommandHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<CreateWorkflowInstanceCommand, int>
{
    public async Task<int> Handle(CreateWorkflowInstanceCommand request, CancellationToken ct)
    {
        var instance = new Workflow
        {
            Status = WorkflowStatus.NotStarted,
            InstanceDataJson = null,
            Originator = request.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId
        };

       dbContext.Workflows.Add(instance);
       await dbContext.SaveChangesAsync(ct);

       return instance.Id;
    }
}