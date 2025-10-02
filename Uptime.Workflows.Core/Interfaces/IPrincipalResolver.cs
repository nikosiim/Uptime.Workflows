using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

/// <summary>
/// Resolves Principal records by SID, creating placeholder entries as needed.
/// </summary>
public interface IPrincipalResolver
{
    Task<Principal?> TryResolveBySidAsync(PrincipalSid sid, CancellationToken ct);

    /// <summary>
    /// Ensures the principal for the given SID exists in the DB (and cache) and returns it.
    /// </summary>
    Task<Principal> ResolveBySidAsync(PrincipalSid sid, CancellationToken ct);

    /// <summary>
    /// Ensures all principals for the given SIDs exist in the DB (and cache).
    /// Does not return data; use for cache-priming before mass task creation.
    /// </summary>
    Task EnsurePrincipalsCachedAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct);

    /// <summary>
    /// Ensures all principals for the given SIDs exist in the DB (and cache), and returns their DTOs.
    /// Use for logging, notifications, or recipient list scenarios.
    /// </summary>
    Task<IReadOnlyList<Principal>> GetPrincipalsBySidAsync(IEnumerable<PrincipalSid> sids, CancellationToken ct);
}