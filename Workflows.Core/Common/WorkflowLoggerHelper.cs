using Microsoft.Extensions.Logging;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Common;

/// <summary>
/// Helper class for standardized logging inside workflows.
/// Ensures consistent format across start, modify, cancel, complete, etc.
/// </summary>
public static class WorkflowLoggerHelper
{
    public static void LogStarted(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Workflow started.",
            definition.Name, workflowId.Value, associationName);
    }

    public static void LogModified(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Workflow modified.",
            definition.Name, workflowId.Value, associationName);
    }

    public static void LogRehydrated(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Workflow rehydrated.",
            definition.Name, workflowId.Value, associationName);
    }

    public static void LogCompleted(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Workflow completed.",
            definition.Name, workflowId.Value, associationName);
    }

    public static void LogCancelled(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Workflow cancelled.",
            definition.Name, workflowId.Value, associationName);
    }
    
    public static void LogFinalStateCancellation(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, BaseState state)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Already in final state '{State}'; no cancellation needed.",
            definition.Name, workflowId.Value, state);
    }
    
    public static void LogAlterTaskTriggered(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId, string? associationName)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Association '{AssociationName}': Alter task triggered.",
            definition.Name, workflowId.Value, associationName);
    }

    public static void LogAlreadyCompletedNoModification(this ILogger logger, IWorkflowDefinition definition, WorkflowId workflowId)
    {
        logger.LogInformation("Workflow {WorkflowName} [{WorkflowId}] - Workflow is already completed. No modifications allowed.",
            definition.Name, workflowId.Value);
    }

    public static void LogTriggerEnqueued<TTrigger>(this ILogger logger, TTrigger trigger, int queueSize)
    {
        logger.LogInformation("Trigger {Trigger} enqueued. Queue size: {QueueSize}", trigger, queueSize);
    }

    public static void LogTriggerProcessingAlreadyInProgress(this ILogger logger)
    {
        logger.LogInformation("Trigger processing is already in progress.");
    }

    public static void LogTriggerProcessingStarted(this ILogger logger)
    {
        logger.LogInformation("Starting trigger queue processing.");
    }

    public static void LogTriggerQueueEmpty(this ILogger logger)
    {
        logger.LogInformation("Trigger queue is empty. Stopping processing.");
    }

    public static void LogTriggerDequeued<TTrigger>(this ILogger logger, TTrigger trigger, int remainingQueueSize)
    {
        logger.LogInformation("Dequeued trigger: {Trigger}. Remaining queue size: {QueueSize}", trigger, remainingQueueSize);
    }

    public static void LogTriggerProcessing<TTrigger, TState>(this ILogger logger, TTrigger trigger, TState currentState)
    {
        logger.LogInformation("Processing trigger {Trigger}. Current state: {CurrentState}", trigger, currentState);
    }

    public static void LogTriggerFired<TTrigger, TState>(this ILogger logger, TTrigger trigger, TState newState)
    {
        logger.LogInformation("Trigger {Trigger} fired successfully. New state: {NewState}", trigger, newState);
    }
}