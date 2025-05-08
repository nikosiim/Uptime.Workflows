using MediatR;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Commands;

public record CreateWorkflowTemplateCommand : IRequest<WorkflowTemplateId>
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required LibraryId LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(WorkflowDbContext context) 
    : IRequestHandler<CreateWorkflowTemplateCommand, WorkflowTemplateId>
{
    public async Task<WorkflowTemplateId> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new WorkflowTemplate
        {
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            LibraryId = request.LibraryId.Value
        };

        context.WorkflowTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return (WorkflowTemplateId)template.Id;
    }
}