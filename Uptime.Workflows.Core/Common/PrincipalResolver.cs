using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Common;

public interface IPrincipalResolver
{
    Task<Principal?> ResolveBySidAsync(string sid, CancellationToken ct);
}

public sealed class PrincipalResolver(WorkflowDbContext db) : IPrincipalResolver
{
    public async Task<Principal?> ResolveBySidAsync(string sid, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return null;

        WorkflowPrincipal? e = await db.Set<WorkflowPrincipal>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalId == sid, ct);
        
        if (e is null) return null;

        return new Principal
        {
            Id = (PrincipalId)e.Id,
            Name = e.Name,
            Email = e.Email
        };
    }
}