using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Commands;

public record CreateWorkflowTemplateCommand : IRequest<Result<WorkflowTemplateId>>
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required LibraryId LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
    public required string SourceSiteUrl { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(WorkflowDbContext db)
    : IRequestHandler<CreateWorkflowTemplateCommand, Result<WorkflowTemplateId>>
{
    public async Task<Result<WorkflowTemplateId>> Handle(CreateWorkflowTemplateCommand request, CancellationToken ct)
    {
        string normalized = SiteUrlValidator.Normalize(request.SourceSiteUrl);

        await SiteUrlValidator.EnsureHostResolvesAsync(normalized, ct);

        var template = new WorkflowTemplate
        {
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            LibraryId = request.LibraryId.Value,
            SiteUrl = normalized
        };

        db.WorkflowTemplates.Add(template);
        await db.SaveChangesAsync(ct);
        
        return Result<WorkflowTemplateId>.Success((WorkflowTemplateId)template.Id);
    }
}