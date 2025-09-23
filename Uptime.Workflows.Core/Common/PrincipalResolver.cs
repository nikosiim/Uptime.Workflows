using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Common;

public interface IPrincipalResolver
{
    Task<Principal?> ResolveBySidAsync(string sid, CancellationToken ct);
}

public sealed class PrincipalResolver(WorkflowDbContext db, ILogger<PrincipalResolver> logger) : IPrincipalResolver
{
    public async Task<Principal?> ResolveBySidAsync(string sid, CancellationToken ct)
    {
        logger.LogInformation("PrincipalResolver called for SID: {Sid}", sid);

        if (string.IsNullOrWhiteSpace(sid))
            return null;

        WorkflowPrincipal? e = await db.Set<WorkflowPrincipal>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalId == sid, ct);

        if (e is null)
        {
            logger.LogWarning("Principal with SID {Sid} not found in DB.", sid);
            return null;
        }

        logger.LogInformation("Principal with SID {Sid} resolved to {Name} ({Email})", sid, e.Name, e.Email);

        return new Principal
        {
            Sid = sid,
            Id = (PrincipalId)e.Id,
            Name = e.Name,
            Email = e.Email
        };
    }
}