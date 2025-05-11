using MediatR;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Commands;

public record CreateWorkflowInstanceCommand : IRequest<WorkflowId>
{
    public WorkflowTemplateId WorkflowTemplateId { get; init; }
    public DocumentId DocumentId { get; init; }
    public string? Originator { get; init; }
}

public class CreateWorkflowInstanceCommandHandler(WorkflowDbContext db)
    : IRequestHandler<CreateWorkflowInstanceCommand, WorkflowId>
{
    public async Task<WorkflowId> Handle(CreateWorkflowInstanceCommand request, CancellationToken ct)
    {
        var instance = new Workflow
        {
            Phase = BaseState.NotStarted.Value,
            StorageJson = null,
            Originator = request.Originator,
            StartDate = DateTime.UtcNow,
            DocumentId = request.DocumentId.Value,
            WorkflowTemplateId = request.WorkflowTemplateId.Value
        };

       db.Workflows.Add(instance);
       await db.SaveChangesAsync(ct);

       return (WorkflowId)instance.Id;
    }
}