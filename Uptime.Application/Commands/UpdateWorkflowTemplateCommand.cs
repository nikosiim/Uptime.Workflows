using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Entities;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Application.Commands;

public record UpdateWorkflowTemplateCommand : IRequest<Result<Unit>>
{
    public WorkflowTemplateId TemplateId { get; init; }
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}

public class UpdateWorkflowTemplateCommandHandler(WorkflowDbContext context)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTemplate? workflowTemplate = await context.WorkflowTemplates
            .FirstOrDefaultAsync(wt => wt.Id == request.TemplateId.Value, cancellationToken);

        if (workflowTemplate == null)
        {
            return Result<Unit>.Failure("WorkflowTemplate not found.");
        }

        workflowTemplate.TemplateName = request.TemplateName;
        workflowTemplate.WorkflowName = request.WorkflowName;
        workflowTemplate.WorkflowBaseId = request.WorkflowBaseId;
        workflowTemplate.AssociationDataJson = request.AssociationDataJson;
        workflowTemplate.Modified = DateTime.UtcNow;

        context.WorkflowTemplates.Update(workflowTemplate);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(new Unit());
    }
}