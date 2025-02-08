using MediatR;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record CreateWorkflowInstanceCommand : IRequest<WorkflowId>
{
    public WorkflowTemplateId WorkflowTemplateId { get; init; }
    public DocumentId DocumentId { get; init; }
    public string? Originator { get; init; }
}

public class CreateWorkflowInstanceCommandHandler(IWorkflowDbContext dbContext)
    : IRequestHandler<CreateWorkflowInstanceCommand, WorkflowId>
{
    public async Task<WorkflowId> Handle(CreateWorkflowInstanceCommand request, CancellationToken ct)
    {
        var instance = new Workflow
        {
            Status = WorkflowStatus.NotStarted,
            StorageJson = null,
            Originator = request.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = request.DocumentId.Value,
            WorkflowTemplateId = request.WorkflowTemplateId.Value
        };

       dbContext.Workflows.Add(instance);
       await dbContext.SaveChangesAsync(ct);

       return (WorkflowId)instance.Id;
    }
}