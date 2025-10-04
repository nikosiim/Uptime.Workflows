using Workflows.Core.Common;
using Workflows.Core.Models;

namespace Workflows.Core.Interfaces;

public interface IWorkflowFactory
{
    IWorkflowMachine? TryGetStateMachine(string workflowBaseId);
    IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId);
    Task<Result<Unit>> StartWorkflowAsync(string workflowBaseId, StartWorkflowPayload payload, CancellationToken ct);
}