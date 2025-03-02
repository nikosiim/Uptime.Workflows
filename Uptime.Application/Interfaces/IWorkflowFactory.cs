using Uptime.Domain.Interfaces;

namespace Uptime.Application.Interfaces;

public interface IWorkflowFactory
{
    IWorkflowMachine? TryGetStateMachine(Guid workflowBaseId);
    IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId);
    Task<string> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
}