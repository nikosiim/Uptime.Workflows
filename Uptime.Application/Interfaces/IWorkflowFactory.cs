using Uptime.Domain.Interfaces;

namespace Uptime.Application.Interfaces;

public interface IWorkflowFactory
{
    Task<string> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken);
    IWorkflowMachine? GetWorkflow(Guid workflowBaseId);
}