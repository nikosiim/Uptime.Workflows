using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowFactory : IWorkflowFactory
{
    private readonly ILogger<WorkflowFactory> _logger;
    private readonly Dictionary<Guid, IWorkflowMachine> _workflowMap;

    public WorkflowFactory(IEnumerable<IWorkflowMachine> workflows, IEnumerable<IWorkflowDefinition> definitions, ILogger<WorkflowFactory> logger)
    {
        _logger = logger;

        Dictionary<Type, IWorkflowDefinition> definitionMap = definitions.ToDictionary(d => d.Type, d => d);

        _workflowMap = workflows
            .Where(workflow => definitionMap.TryGetValue(workflow.GetType(), out _))
            .ToDictionary(workflow => Guid.Parse(definitionMap[workflow.GetType()].Id), workflow => workflow);
    }

    public async Task<string> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (_workflowMap.TryGetValue(payload.WorkflowBaseId, out IWorkflowMachine? workflow))
        {
            BaseState phase = await workflow.StartAsync(payload, cancellationToken);
            return phase.Value;
        }

        _logger.LogWarning("No workflow found for WorkflowBaseId: {WorkflowBaseId}", payload.WorkflowBaseId);
        return BaseState.Invalid.Value;
    }

    public IWorkflowMachine? GetWorkflow(Guid workflowBaseId)
    {
        return _workflowMap.GetValueOrDefault(workflowBaseId);
    }
}