using Uptime.Application.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowMachine
{
    Task FireAsync(string phaseName, WorkflowTrigger trigger);
    Task UpdateWorkflowStateAsync();
}