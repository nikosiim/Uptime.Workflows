﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Application.Commands;

public record DeleteWorkflowTemplateCommand(WorkflowTemplateId TemplateId) : IRequest<Result<Unit>>;

public class DeleteWorkflowTemplateCommandHandler(WorkflowDbContext context, ILogger<DeleteWorkflowTemplateCommand> logger)
    : IRequestHandler<DeleteWorkflowTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        WorkflowTemplate? template = await context.WorkflowTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, cancellationToken);
        if (template == null) 
            return Result<Unit>.Failure($"WorkflowTemplate with ID {request.TemplateId.Value} not found.");
        
        bool hasWorkflows = await context.Workflows.AnyAsync(w => w.WorkflowTemplateId == request.TemplateId.Value, cancellationToken: cancellationToken);
        if (hasWorkflows)
            return Result<Unit>.Failure("Cannot delete the template as it is being used by existing workflows.");

        template.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Workflow template with ID {TemplateId} deleted.", request.TemplateId);
        
        return Result<Unit>.Success(new Unit());
    }
}