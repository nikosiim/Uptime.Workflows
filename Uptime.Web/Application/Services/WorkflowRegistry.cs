using Uptime.Web.Application.Definitions;

namespace Uptime.Web.Application.Services;

public class WorkflowRegistry
{
    private readonly Dictionary<string, IWorkflowDefinition> _workflows = new();

    public WorkflowRegistry(IEnumerable<IWorkflowDefinition> workflows)
    {
        foreach (IWorkflowDefinition workflow in workflows)
        {
            _workflows[workflow.Id] = workflow;
        }
    }

    /// <summary>
    /// Gets a workflow definition by its ID.
    /// </summary>
    public IWorkflowDefinition GetWorkflowById(string id)
    {
        if (_workflows.TryGetValue(id, out IWorkflowDefinition? workflow))
        {
            return workflow;
        }

        throw new Exception($"Workflow with ID {id} not found.");
    }

    /// <summary>
    /// Gets all registered workflows.
    /// </summary>
    public IEnumerable<IWorkflowDefinition> GetAllWorkflows() => _workflows.Values;
}