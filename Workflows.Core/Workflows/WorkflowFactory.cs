using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Workflows.Core.Common;
using Workflows.Core.Interfaces;
using Workflows.Core.Models;

namespace Workflows.Core;

/// <summary>
/// Centrally resolves <c>IWorkflowMachine</c> instances by their
/// **workflow-base GUID** (<see cref="IWorkflowDefinition.Id"/>).
///
/// • ASP.NET and the console harness inject all available workflow machines
///   (<see cref="IWorkflowMachine"/>) **and** their definitions
///   (<see cref="IWorkflowDefinition"/>) at startup.
///
/// • During construction we validate that every definition’s <c>Id</c> is a
///   real <see cref="Guid"/> and build two read-only lookup tables:
///   <list type="bullet">
///     <item>
///       <description>
///       <c>_definitionsByBaseId</c>  – <c>Guid → IWorkflowDefinition</c>
///       </description>
///     </item>
///     <item>
///       <description>
///       <c>_machinesByBaseId</c>      – <c>Guid → IWorkflowMachine</c>
///       (only if a matching definition exists)
///       </description>
///     </item>
///   </list>
///
/// Why do we need this class?
/// <para>
/// Incoming commands / API calls know **only the GUID** that lives in the
/// database (<c>WorkflowTemplate.WorkflowBaseId</c>).  They have no idea which
/// concrete C# type implements the state-machine.  <b>WorkflowFactory</b>
/// hides that mapping so application code can simply call
/// <see cref="StartWorkflowAsync"/> / <see cref="TryGetStateMachine"/> without
/// a giant switch-statement or service locator.
/// </para>
/// </summary>
public class WorkflowFactory : IWorkflowFactory
{
    private readonly ILogger<WorkflowFactory> _logger;

    private readonly ReadOnlyDictionary<Guid, IWorkflowMachine> _machinesByBaseId;
    private readonly ReadOnlyDictionary<Guid, IWorkflowDefinition> _definitionsByBaseId;

    /// <param name="workflows">
    /// All concrete workflow machines registered in DI
    /// (e.g. <c>ApprovalWorkflow</c>, <c>SigningWorkflow</c>, …).
    /// </param>
    /// <param name="definitions">
    /// One definition per workflow machine; provides the GUID and metadata.
    /// </param>
    /// <param name="logger"></param>
    /// <remarks>
    /// Both collections are usually added with
    /// <c>services.AddScoped&lt;IWorkflowMachine, MyWorkflow&gt;()</c> and
    /// <c>services.AddSingleton&lt;IWorkflowDefinition, MyDefinition&gt;()</c>.
    /// </remarks>
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
    
    public IWorkflowMachine? TryGetStateMachine(string workflowBaseId) 
        => _machinesByBaseId.GetValueOrDefault(new Guid(workflowBaseId));

    public IWorkflowDefinition? TryGetDefinition(Guid workflowBaseId) 
        => _definitionsByBaseId.GetValueOrDefault(workflowBaseId);
    
    /// <summary>
    /// Starts the workflow represented by <paramref name="payload"/>.
    /// </summary>
    /// <remarks>
    /// Fails if the GUID has no matching machine.  
    /// The caller (API / command handler) converts that failure into an HTTP
    /// 400 or similar.
    /// </remarks>
    public async Task<Result<Unit>> StartWorkflowAsync(string workflowBaseIdString, StartWorkflowPayload payload, CancellationToken ct)
    {
        if (_machinesByBaseId.TryGetValue(new Guid(workflowBaseIdString), out IWorkflowMachine? machine))
        {
            return await machine.StartAsync(payload, ct);
        }

        _logger.LogWarning("State-machine not found for baseId: {WorkflowBaseId}", workflowBaseIdString);
        return Result<Unit>.Failure(ErrorCode.NotFound);
    }
}