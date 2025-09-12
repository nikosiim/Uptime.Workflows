using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public interface IWorkflowFactory
{
    IWorkflowMachine? TryGetStateMachine(string workflowBaseId);
    IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId);
    Task<Result<Unit>> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
}