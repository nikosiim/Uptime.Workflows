using Uptime.Application.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowMachine
{
    /// <summary>
    /// Fires a workflow trigger for the given phase.
    /// </summary>
    Task FireAsync(string phaseName, WorkflowTrigger trigger);
}