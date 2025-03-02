using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowFactory : IWorkflowFactory
{
    private readonly ILogger<WorkflowFactory> _logger;
    private readonly ReadOnlyDictionary<Guid, IWorkflowMachine> _machinesByBaseId;
    private readonly ReadOnlyDictionary<Guid, IWorkflowDefinition> _definitionsByBaseId;

    public WorkflowFactory(IEnumerable<IWorkflowMachine> workflows, IEnumerable<IWorkflowDefinition> definitions, ILogger<WorkflowFactory> logger)
    {
        _logger = logger;

        var typeToDefinition = new Dictionary<Type, IWorkflowDefinition>();
        var definitionsById = new Dictionary<Guid, IWorkflowDefinition>();

        // Validate and index all definitions
        foreach (IWorkflowDefinition def in definitions)
        {
            if (!Guid.TryParse(def.Id, out Guid defGuid))
            {
                throw new InvalidOperationException($"Definition '{def.Name}' has invalid GUID '{def.Id}'.");
            }
            typeToDefinition[def.Type] = def;
            definitionsById[defGuid] = def;
        }

        // Build machine map
        var machinesById = new Dictionary<Guid, IWorkflowMachine>();

        foreach (IWorkflowMachine machine in workflows)
        {
            if (!typeToDefinition.TryGetValue(machine.GetType(), out IWorkflowDefinition? def))
            {
                _logger.LogWarning("No matching definition for machine type {Type}", machine.GetType());
                continue;
            }

            Guid defGuid = Guid.Parse(def.Id); // guaranteed to work since we validated above
            machinesById[defGuid] = machine;
        }

        // Store as read-only
        _machinesByBaseId = new ReadOnlyDictionary<Guid, IWorkflowMachine>(machinesById);
        _definitionsByBaseId = new ReadOnlyDictionary<Guid, IWorkflowDefinition>(definitionsById);
    }
    
    public IWorkflowMachine? TryGetStateMachine(Guid workflowBaseId) 
        => _machinesByBaseId.GetValueOrDefault(workflowBaseId);

    public IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId) 
        => _definitionsByBaseId.GetValueOrDefault(workflowBaseId);
    
    public async Task<string> StartWorkflowAsync(IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (_machinesByBaseId.TryGetValue(payload.WorkflowBaseId, out IWorkflowMachine? machine))
        {
            BaseState phase = await machine.StartAsync(payload, cancellationToken);
            return phase.Value;
        }

        _logger.LogWarning("No workflow machine found for baseId: {WorkflowBaseId}", payload.WorkflowBaseId);
        return BaseState.Invalid.Value;
    }
}