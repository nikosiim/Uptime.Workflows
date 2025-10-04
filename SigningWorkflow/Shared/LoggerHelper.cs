using Microsoft.Extensions.Logging;
using Workflows.Core.Common;
using Workflows.Core.Interfaces;

namespace SigningWorkflow;

/// <summary>
/// Helper class for standardized logging inside workflow, to ensure consistent format.
/// </summary>
internal static class LoggerHelper
{
    
    internal static void LogSigningTaskCreated(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Signing task created.",
            definition.Name, workflowId.Value, associationName);
    }

    internal static void LogSigningTaskCompleted(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Signing task completed.",
            definition.Name, workflowId.Value, associationName);
    }
}
