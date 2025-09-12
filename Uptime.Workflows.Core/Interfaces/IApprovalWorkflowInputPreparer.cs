namespace Uptime.Workflows.Core.Interfaces;

public interface IApprovalWorkflowInputPreparer
{
    Task PrepareAsync(Dictionary<string, string?> storage, CancellationToken ct);
}