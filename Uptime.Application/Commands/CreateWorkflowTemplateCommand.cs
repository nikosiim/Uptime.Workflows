using MediatR;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;

namespace Uptime.Application.Commands;

public class CreateWorkflowTemplateCommand : IRequest<int>
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required int LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(IWorkflowDbContext context) 
    : IRequestHandler<CreateWorkflowTemplateCommand, int>
{
    public async Task<int> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new WorkflowTemplate
        {
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            LibraryId = request.LibraryId
        };

        context.WorkflowTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}