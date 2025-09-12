using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using static ApprovalWorkflow.Constants;

namespace ApprovalWorkflow;

public sealed class ApprovalWorkflowInputPreparer(IPrincipalResolver resolver) : IApprovalWorkflowInputPreparer
{
    public async Task PrepareAsync(Dictionary<string, string?> storage, CancellationToken ct)
    {
        if (storage.TryGetValueAsList(TaskStorageKeys.TaskExecutorsSid, out List<string> execSids) && execSids.Count > 0)
        {
            (List<PrincipalId> foundExecs, List<string> missingExecs) = await ResolveManyAsync(resolver, execSids, ct);
            if (missingExecs.Count > 0)
                throw new WorkflowValidationException(ErrorCode.NotFound, $"Executors not found for SID(s): {string.Join(", ", missingExecs)}");

            storage.SetValue(TaskStorageKeys.TaskExecutorsPrincipalIds, string.Join(",", foundExecs.Select(id => id.Value)));
        }

        if (storage.TryGetValueAsList(TaskStorageKeys.TaskSignersSid, out List<string> signerSids) && signerSids.Count > 0)
        {
            (List<PrincipalId> foundSigners, List<string> missingSigners) = await ResolveManyAsync(resolver, signerSids, ct);
            if (missingSigners.Count > 0)
                throw new WorkflowValidationException(ErrorCode.NotFound, $"Signers not found for SID(s): {string.Join(", ", missingSigners)}");

            storage.SetValue(TaskStorageKeys.TaskSignersPrincipalIds, string.Join(",", foundSigners.Select(id => id.Value)));
        }
    }

    private static async Task<(List<PrincipalId> found, List<string> missing)> ResolveManyAsync(
        IPrincipalResolver resolver, IEnumerable<string> sids, CancellationToken ct)
    {
        var found = new List<PrincipalId>();
        var missing = new List<string>();

        // Resolve in parallel (keeps your current resolver API)
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