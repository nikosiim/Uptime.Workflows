using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<Result<Unit>>
{
    public WorkflowTemplateId TemplateId { get; init; }
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(WorkflowDbContext db)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateWorkflowTemplateCommand request, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTemplate? workflowTemplate = await db.WorkflowTemplates
            .FirstOrDefaultAsync(wt => wt.Id == request.TemplateId.Value, ct);

        if (workflowTemplate is null)
            return Result<Unit>.Failure(ErrorCode.NotFound);

        workflowTemplate.TemplateName = request.TemplateName;
        workflowTemplate.WorkflowName = request.WorkflowName;
        workflowTemplate.WorkflowBaseId = request.WorkflowBaseId;
        workflowTemplate.AssociationDataJson = request.AssociationDataJson;
        workflowTemplate.Modified = DateTime.UtcNow;

        db.WorkflowTemplates.Update(workflowTemplate);
        await db.SaveChangesAsync(ct);

        return Result<Unit>.Success(new Unit());
    }
}