using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public static class WorkflowPrincipalResolver
{
    public static async Task<Principal> ResolvePrincipalBySidAsync(IPrincipalResolver resolver, string? sid, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sid))
            throw new WorkflowValidationException(ErrorCode.NotFound, "SID not provided");

        Principal? principal = await resolver.ResolveBySidAsync(sid, ct);

        return principal ?? throw new WorkflowValidationException(ErrorCode.NotFound, $"Not found for SID: {sid}");
    }

    public static async Task ResolveAndStorePrincipalIdsAsync(
        Func<IWorkflowContext, List<string>> getSids,
        Action<IWorkflowContext, IEnumerable<string>> setPrincipalIds,
        IWorkflowContext context,
        IPrincipalResolver resolver,
        CancellationToken ct)
    {
        List<string> sids = getSids(context);
        if (sids.Count == 0) return;

        (List<PrincipalId> found, List<string> missing) = await ResolveManyAsync(resolver, sids, ct);
        if (missing.Count > 0)
            throw new WorkflowValidationException(ErrorCode.NotFound, $"Not found for SID(s): {string.Join(", ", missing)}");

        setPrincipalIds(context, found.Select(id => id.ToString()));
    }

    private static async Task<(List<PrincipalId> found, List<string> missing)> ResolveManyAsync(
        IPrincipalResolver resolver, IEnumerable<string> sids, CancellationToken ct)
    {
        var found = new List<PrincipalId>();
        var missing = new List<string>();

        IEnumerable<Task<(string sid, Principal? p)>> tasks = sids.Select(async sid =>
        {
            Principal? p = await resolver.ResolveBySidAsync(sid, ct);
            return (sid, p);
        });

        foreach ((string sid, Principal? principal) in await Task.WhenAll(tasks))
        {
            if (principal is null) missing.Add(sid);
            else found.Add(principal.Id);
        }

        return (found, missing);
    }
}