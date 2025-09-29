using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowFactory
{
    IWorkflowMachine? TryGetStateMachine(string workflowBaseId);
    IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId);
    Task<Result<Unit>> StartWorkflowAsync(string workflowBaseId, StartWorkflowPayload payload, CancellationToken ct);
}