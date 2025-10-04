using Microsoft.EntityFrameworkCore;
using Workflows.Core.Common;
using Workflows.Core.Data;
using Workflows.Core.Enums;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Core.Services;

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
            PerformedById = executor.Id.Value
        };
        
        await using WorkflowDbContext db = await factory.CreateDbContextAsync(ct);

        db.WorkflowHistories.Add(historyEntry);
        await db.SaveChangesAsync(ct);
    }
}