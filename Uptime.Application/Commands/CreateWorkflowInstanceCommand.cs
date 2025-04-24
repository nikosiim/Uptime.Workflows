using MediatR;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Entities;

namespace Uptime.Application.Commands;

public record CreateWorkflowInstanceCommand : IRequest<WorkflowId>
{
    public WorkflowTemplateId WorkflowTemplateId { get; init; }
    public DocumentId DocumentId { get; init; }
    public string? Originator { get; init; }
}

public class CreateWorkflowInstanceCommandHandler(WorkflowDbContext dbContext)
    : IRequestHandler<CreateWorkflowInstanceCommand, WorkflowId>
{
    public async Task<WorkflowId> Handle(CreateWorkflowInstanceCommand request, CancellationToken cancellationToken)
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

       dbContext.Workflows.Add(instance);
       await dbContext.SaveChangesAsync(cancellationToken);

       return (WorkflowId)instance.Id;
    }
}