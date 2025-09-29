using Microsoft.Extensions.Logging;
using System.Threading;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

// -----------------------------------------------------------------------------
// ReplicatorWorkflowBase<TContext>
//
// Base class for workflows that include one or more "replicator" phases.
// A replicator phase fans out work to multiple parallel child activities
// (e.g., sending an approval task to each approver). The replicator tracks
// the state of all child items and signals the workflow when the phase is
// complete.
//
// Key concepts:
//   • ReplicatorState  – runtime state of one phase, including all items.
//   • ReplicatorItem   – a single pending/active/completed child.
//   • ReplicatorManager– orchestrates execution of replicator phases.
//   • Child activity   – the actual workflow activity run for each item.
//
// Hook-based design:
//   Unlike the previous provider-based model, workflows now override virtual
//   methods directly instead of supplying an IReplicatorActivityProvider.
//
//   Override points:
//     CreateChildActivity(ctx)    – create a concrete IUserTaskActivity for a
//                                   given item. Used in AlterTask path.
//     CreateActivityForReplicator(ctx)
//                                 – create the IWorkflowActivity instance for
//                                   the replicator engine. Default maps to
//                                   CreateChildActivity.
//     OnChildInitialized(phaseId, ctx, act)
//                                 – invoked when a new child activity has been
//                                   created and initialized.
//     OnChildCompleted(phaseId, act)
//                                 – invoked when a user task activity is marked
//                                   completed (including reject/delegate cases).
//
// AlterTask path:
//   OnTaskAlteredAsync(...) calls CreateChildActivity to reconstruct the
//   activity for the given task, invokes ChangedTaskAsync, and then calls
//   OnChildCompleted + RunReplicatorAsync if the task finished.
//
// Usage pattern:
//   • Derive your workflow from ReplicatorWorkflowBase<TContext>.
//   • Override the hooks to decide what activity to run per phase, and to
//     handle special logic when a task completes (delegation, rejection, etc.).
//   • Configure your state machine to call RunReplicatorAsync(...) on entry
//     to each replicator phase.
// -----------------------------------------------------------------------------
public abstract class ReplicatorWorkflowBase<TContext>(
    IWorkflowService workflowService,
    ITaskService taskService,
    IHistoryService historyService,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(workflowService, taskService, historyService, logger)
    where TContext : class, IReplicatorWorkflowContext, new()
{
    private ReplicatorManager? _replicatorManager;
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;
    
    protected override async Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        // Ensure SIDs are resolved to PrincipalIds before building phases
        await base.OnWorkflowActivatedAsync(ct);

        IReplicatorPhaseBuilder builder = CreateReplicatorPhaseBuilder();
        List<ReplicatorPhase> replicatorPhases = builder.BuildPhases(WorkflowContext);

        // Convert phases into replicator states and store them in the workflow context.
        WorkflowContext.ReplicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState
            {
                ReplicatorType = phase.Type,
                Items = phase.TaskContext.Select(activityContext => new ReplicatorItem(activityContext)).ToList()
            }
        );

        InitializeReplicatorManagerAsync(ct);
    }

    protected override string OnWorkflowModification()
    {
        var modificationContext = string.Empty;

        if (!WorkflowContext.ReplicatorStates.TryGetValue(Machine.State.Value, out ReplicatorState? replicatorState))
        {
            _logger.LogWarning("Workflow {WorkflowId} Replicator phase not found {phaseId}", WorkflowId, Machine.State.Value);
            return modificationContext;
        }

        if (replicatorState.ReplicatorType == ReplicatorType.Parallel)
        {
            _logger.LogWarning("Workflow {WorkflowId} update not allowed for parallel workflows", WorkflowId);
            return modificationContext;
        }

        if (!replicatorState.HasActiveItems)
        {
            _logger.LogWarning("Workflow {WorkflowId} update not allowed in this phase", WorkflowId);
        }

        return modificationContext;
    }
    
    protected override async Task OnTaskAlteredAsync(WorkflowEventType action, WorkflowActivityContext activityContext, PrincipalSid executorSid, 
        Dictionary<string, string?> inputData, CancellationToken ct)
    {
        IUserTaskActivity? taskActivity = CreateChildActivity(activityContext);
        if (taskActivity is null)
            throw new InvalidOperationException("Current task is not a user-interrupting activity.");

        if (!taskActivity.IsCompleted)
        {
            await taskActivity.ChangedTaskAsync(action, activityContext, executorSid, inputData, ct);
            
            string? phase = WorkflowContext.ReplicatorStates.FindPhase(activityContext.TaskGuid);

            if (taskActivity.IsCompleted && !string.IsNullOrWhiteSpace(phase))
            {
                OnChildCompleted(phase, taskActivity);
                UpdateWorkflowContextReplicatorState(activityContext.TaskGuid, ReplicatorItemStatus.Completed);

                await RunReplicatorAsync(phase, ct);
            }
        }
    }

    protected virtual IUserTaskActivity? CreateChildActivity(IWorkflowActivityContext ctx) => null;

    protected virtual IWorkflowActivity CreateActivityForReplicator(IWorkflowActivityContext ctx)
    {
        IUserTaskActivity? userTask = CreateChildActivity(ctx);
        return userTask ?? throw new InvalidOperationException("Phase requires an activity but CreateChildActivity returned null.");
    }

    /// <summary>
    /// Creates the phase builder used by this workflow to describe its replicator phases.
    /// <para>
    /// <b>Why is this needed?</b><br/>
    /// Each workflow may have a different set of phases (e.g., Approval, Signing), with
    /// different data and logic for generating tasks in each phase. By overriding this method,
    /// a derived workflow (like ApprovalWorkflow) can plug in the right configuration.
    /// </para>
    /// <para>
    /// <b>What does the phase builder do?</b><br/>
    /// It holds the phase configuration (how to generate tasks, what type of replicator to use, etc),
    /// and can generate all required phases/tasks for a given workflow context at runtime.
    /// </para>
    /// <para>
    /// <b>See also:</b> <see cref="ReplicatorPhaseBuilder"/> for how phases are built at runtime.
    /// </para>
    /// </summary>
    protected virtual IReplicatorPhaseBuilder CreateReplicatorPhaseBuilder()
    {
        Dictionary<string, ReplicatorPhaseConfiguration> replicatorPhaseConfiguration = WorkflowDefinition.ReplicatorConfiguration!.PhaseConfigurations;
        return new ReplicatorPhaseBuilder(replicatorPhaseConfiguration);
    }
    
    protected virtual void UpdateWorkflowContextReplicatorState(Guid taskGuid, ReplicatorItemStatus status)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(taskGuid);
        if (item != null)
        {
            item.Status = status;
        }
    }

    protected virtual void OnChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity) { }

    protected virtual void OnChildCompleted(string phaseId, IUserTaskActivity activity) { }

    protected async Task RunReplicatorAsync(string phaseName, CancellationToken ct)
    {
        InitializeReplicatorManagerAsync(ct);
        await _replicatorManager!.RunReplicatorAsync(phaseName, ct);
    }

    private void InitializeReplicatorManagerAsync(CancellationToken ct)
    {
        if (_replicatorManager == null)
        {
            _replicatorManager = new ReplicatorManager();
            _replicatorManager.LoadReplicators(
                WorkflowContext.ReplicatorStates,
                createActivity: item => CreateActivityForReplicator(item.ActivityContext),
                onChildInitialized: OnChildInitialized,
                onAllTasksCompleted: () => TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, ct));
        }
    }
}