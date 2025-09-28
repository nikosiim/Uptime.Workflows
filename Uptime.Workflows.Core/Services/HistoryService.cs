using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public class HistoryService(IDbContextFactory<WorkflowDbContext> factory, IPrincipalResolver principalResolver) : IHistoryService
{
    public async Task CreateAsync(WorkflowId workflowId, WorkflowEventType eventType, PrincipalSid executorSid, 
        string? description, string? comment = null, CancellationToken ct = default)
    {
        Principal executor = await principalResolver.ResolveBySidAsync(executorSid, ct);

        var historyEntry = new WorkflowHistory
        {
            Event = eventType,
            Occurred = DateTime.UtcNow,
            Description = description,
            Comment = comment,
            WorkflowId = workflowId.Value,
            PerformedByPrincipalId = executor.Id.Value
        };
        
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        db.WorkflowHistories.Add(historyEntry);
        await db.SaveChangesAsync(ct);
    }
}