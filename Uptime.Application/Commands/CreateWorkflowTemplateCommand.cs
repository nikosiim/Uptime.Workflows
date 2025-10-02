using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Application.Commands;

public record CreateWorkflowTemplateCommand : IRequest<Result<WorkflowTemplateId>>, IRequiresPrincipal
{
    public required PrincipalSid ExecutorSid { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required Guid LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
    public required string SourceSiteUrl { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(WorkflowDbContext db, IPrincipalResolver principalResolver)
    : IRequestHandler<CreateWorkflowTemplateCommand, Result<WorkflowTemplateId>>
{
    public async Task<Result<WorkflowTemplateId>> Handle(CreateWorkflowTemplateCommand request, CancellationToken ct)
    {
        string normalized = SiteUrlValidator.Normalize(request.SourceSiteUrl);
        await SiteUrlValidator.EnsureHostResolvesAsync(normalized, ct);

        Principal executor = await principalResolver.ResolveBySidAsync(request.ExecutorSid, ct);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var template = new WorkflowTemplate
        {
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson,
            LibraryId = request.LibraryId,
            SiteUrl = normalized,
            CreatedAtUtc = now,
            CreatedByPrincipalId = executor.Id.Value,
            UpdatedAtUtc = null,
            UpdatedByPrincipalId = null,
            IsDeleted = false
        };

        db.WorkflowTemplates.Add(template);
        await db.SaveChangesAsync(ct);
        
        return Result<WorkflowTemplateId>.Success((WorkflowTemplateId)template.Id);
    }
}