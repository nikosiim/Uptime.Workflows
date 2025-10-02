using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<Result<Unit>>, IRequiresPrincipal
{
    public required PrincipalSid ExecutorSid { get; init; }
    public WorkflowTemplateId TemplateId { get; init; }
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(WorkflowDbContext db, IPrincipalResolver principalResolver)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateWorkflowTemplateCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTemplate? template = await db.WorkflowTemplates
            .FirstOrDefaultAsync(wt => wt.Id == request.TemplateId.Value, ct);

        if (template is null)
            return Result<Unit>.Failure(ErrorCode.NotFound, "Workflow template not found.");

        Principal executor = await principalResolver.ResolveBySidAsync(request.ExecutorSid, ct);

        template.TemplateName = request.TemplateName;
        template.WorkflowName = request.WorkflowName;
        template.WorkflowBaseId = request.WorkflowBaseId;
        template.AssociationDataJson = request.AssociationDataJson;
        template.UpdatedAtUtc = DateTimeOffset.UtcNow;
        template.UpdatedByPrincipalId = executor.Id.Value;

        db.WorkflowTemplates.Update(template);
        await db.SaveChangesAsync(ct);

        return Result<Unit>.Success(new Unit());
    }
}