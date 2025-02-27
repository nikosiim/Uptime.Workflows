using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public static class WorkflowHelper
{
    public static bool IsTaskButtonVisible(this WorkflowState workflowState, WorkflowTaskDetails task, string buttonName)
    {
        Result<List<WorkflowDefinition>> workflowDefinitionResult = workflowState.WorkflowDefinitionsQuery.Result;

        WorkflowDefinition? workflowDefinition = workflowDefinitionResult.Value?.FirstOrDefault(wd => wd.Id == task.WorkflowBaseId);
        if (workflowDefinition == null)
            return false; // If the workflow is not found, button should be hidden

        // Check for replicator activities
        PhaseActivity? phaseActivity = workflowDefinition.ReplicatorActivities?.FirstOrDefault(pa => pa.PhaseId == task.PhaseId);
        if (phaseActivity is { Actions: not null })
        {
            return phaseActivity.Actions.Contains(buttonName); // If action is present, button is visible
        }

        // If no replicator activity is found, check workflow definition actions
        return workflowDefinition.Actions != null && workflowDefinition.Actions.Contains(buttonName);
    }
}