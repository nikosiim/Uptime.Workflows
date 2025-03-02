using System.Text.Json;
using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public static class WorkflowReplicatorHelper
{
    public static (IReplicatorWorkflowContext? Context, PhaseActivity? Activity, ReplicatorState? State) 
        ResolveReplicatorPhaseData(Workflow workflowInstance, IWorkflowFactory workflowFactory, ILogger logger)
    {
        var baseGuid = new Guid(workflowInstance.WorkflowTemplate.WorkflowBaseId);
        
        IWorkflowDefinition? definition = workflowFactory.TryGetDefinition(baseGuid);
        if (definition == null)
        {
            logger.LogWarning("No definition for Workflow {Id}", workflowInstance.Id);
            return (null, null, null);
        }

        PhaseActivity? phaseActivity = definition.ReplicatorConfiguration?.PhaseActivities.FirstOrDefault(a => a.PhaseId == workflowInstance.Phase);
        if (phaseActivity is null)
        {
            logger.LogWarning("N o PhaseActivity found for phase '{Phase}' in workflow {Id}", workflowInstance.Phase, workflowInstance.Id);
            return (null, null, null);
        }

        var ctx = WorkflowContextHelper.Deserialize(definition.ContextType, workflowInstance.StorageJson) as IWorkflowContext;
        if (ctx is not IReplicatorWorkflowContext replicatorContext)
        {
            logger.LogWarning("Workflow {Id} does not implement IReplicatorWorkflowContext", workflowInstance.Id);
            return (null, null, null);
        }

        if (!replicatorContext.ReplicatorStates.TryGetValue(workflowInstance.Phase, out ReplicatorState? replicatorState))
        {
            logger.LogWarning("No ReplicatorState found for phase '{Phase}' in workflow {Id}", workflowInstance.Phase, workflowInstance.Id);
            return (null, null, null);
        }

        return (replicatorContext, phaseActivity, replicatorState);
    }

    public static IEnumerable<(Guid TaskGuid, string AssignedTo)> OfTypeUserTaskActivity(this IEnumerable<ReplicatorItem> items)
    {
        foreach (ReplicatorItem item in items)
        {
            if (item.Data is JsonElement { ValueKind: JsonValueKind.Object } jsonElement)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var typedData = JsonSerializer.Deserialize<UserTaskActivityData>(jsonElement.GetRawText(), options);

                if (typedData != null)
                {
                    yield return (item.TaskGuid, typedData.AssignedTo);
                }
            }
        }
    }
}