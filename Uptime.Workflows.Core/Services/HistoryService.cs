﻿using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Services;

public class HistoryService(IDbContextFactory<WorkflowDbContext> factory) : IHistoryService
{
    public async Task CreateAsync(
        WorkflowId workflowId,
        WorkflowEventType eventType,
        string? author,
        string? description,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var historyEntry = new WorkflowHistory
        {
            Event = eventType,
            User = author,
            Occurred = DateTime.UtcNow,
            Description = description,
            Comment = comment,
            WorkflowId = workflowId.Value
        };

        await using WorkflowDbContext db = await factory.CreateDbContextAsync(cancellationToken);

        db.WorkflowHistories.Add(historyEntry);
        await db.SaveChangesAsync(cancellationToken);
    }
}