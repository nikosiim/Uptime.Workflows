using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Interfaces;

public interface IWorkflowFactory
{
    IWorkflowMachine? TryGetStateMachine(string workflowBaseId);
    IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId);
    Task<Result<Unit>> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
}