using Uptime.Application.Enums;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowMachine
{
    Task<WorkflowPhase> StartAsync(IWorkflowPayload payload);
    Task FireAsync(string phaseName, WorkflowTrigger trigger);
    Task UpdateWorkflowStateAsync();
    Task<bool> ReHydrateAsync(WorkflowId workflowId);
    Task<WorkflowPhase> TryAlterTaskAsync(IAlterTaskPayload payload);
}