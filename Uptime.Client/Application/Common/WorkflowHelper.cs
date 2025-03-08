using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Workflow;

namespace Uptime.Client.Application.Common;

public static class WorkflowHelper
{
    public static bool IsTaskButtonVisible(this WorkflowState workflowState, WorkflowTaskDetails task, string buttonName)
    {
        Result<List<WorkflowDefinition>> workflowDefinitionResult = workflowState.WorkflowDefinitionsQuery.Result;

        WorkflowDefinition? workflowDefinition = workflowDefinitionResult.Value?.FirstOrDefault(wd => wd.Id == task.WorkflowBaseId);
        if (workflowDefinition == null)
            return false; // If the workflow is not found, button should be hidden

        if (task.AssignedTo != workflowState.CurrentUser.Name && 
            task.AssignedBy != workflowState.CurrentUser.Name && 
            !workflowState.CurrentUser.IsAdmin) 
            return false;

        // Check for replicator activities
        PhaseActivity? phaseActivity = workflowDefinition.ReplicatorActivities?.FirstOrDefault(pa => pa.PhaseId == task.PhaseId);
        if (phaseActivity is { Actions: not null })
        {
            return phaseActivity.Actions.Contains(buttonName); // If action is present, button is visible
        }

        // If no replicator activity is found, check workflow definition actions
        return workflowDefinition.Actions != null && workflowDefinition.Actions.Contains(buttonName);
    }

    public static bool IsWorkflowUpdateEnabled(this WorkflowState workflowState, string workflowBaseId, string currentPhase)
    {
        if (!workflowState.CurrentUser.IsAdmin) 
            return false;

        Result<List<WorkflowDefinition>> workflowDefinitionResult = workflowState.WorkflowDefinitionsQuery.Result;
        WorkflowDefinition? workflowDefinition = workflowDefinitionResult.Value?.FirstOrDefault(wd => wd.Id == workflowBaseId);
        PhaseActivity? phaseActivity = workflowDefinition?.ReplicatorActivities?.FirstOrDefault(pa => pa.PhaseId == currentPhase);

        return phaseActivity is { UpdateEnabled: true };
    }
}