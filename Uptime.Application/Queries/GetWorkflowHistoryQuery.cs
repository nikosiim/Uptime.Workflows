﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Application.Queries;

public record GetWorkflowHistoryQuery(WorkflowId WorkflowId) : IRequest<List<WorkflowHistoryDto>>;

public class GetWorkflowHistoryQueryHandler(WorkflowDbContext dbContext) : IRequestHandler<GetWorkflowHistoryQuery, List<WorkflowHistoryDto>>
{
    public async Task<List<WorkflowHistoryDto>> Handle(GetWorkflowHistoryQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowHistories.AsNoTracking()
            .Where(x => x.WorkflowId == request.WorkflowId.Value)
            .Select(history => new WorkflowHistoryDto
            {
                Id = history.Id,
                WorkflowId = history.WorkflowId,
                Description = history.Description,
                Event = history.Event,
                Occurred = history.Occurred,
                Comment = history.Comment,
                User = history.User
            })
            .ToListAsync(cancellationToken);
    }
}