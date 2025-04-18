﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Unit = Uptime.Domain.Common.Unit;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand : IRequest<Result<Unit>>
{
    public required string Originator { get; init; }
    public required DocumentId DocumentId { get; init; }
    public required WorkflowTemplateId WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class StartWorkflowCommandHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory)
    : IRequestHandler<StartWorkflowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<Unit>.Cancelled();

        string workflowBaseIdString = await dbContext.WorkflowTemplates
            .Where(wt => wt.Id == request.WorkflowTemplateId.Value)
            .Select(wt => wt.WorkflowBaseId)
            .FirstAsync(cancellationToken);

        var payload = new StartWorkflowPayload
        {
            WorkflowBaseId = new Guid(workflowBaseIdString),
            Originator = request.Originator,
            DocumentId = request.DocumentId,
            WorkflowTemplateId = request.WorkflowTemplateId,
            Storage = request.Storage
        };

        return await workflowFactory.StartWorkflowAsync(payload, cancellationToken);
    }
}