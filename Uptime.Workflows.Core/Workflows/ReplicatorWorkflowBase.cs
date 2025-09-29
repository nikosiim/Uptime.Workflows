using Microsoft.Extensions.Logging;
using System.Threading;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

/// <summary>
/// Base class for workflows that contain "replicator" phases (i.e., phases that dynamically generate
/// a set of child activities for multiple users — such as approval tasks or signings, possibly in parallel).
///
/// <para>
/// This class provides all the boilerplate for:
/// <list type="bullet">
///   <item>Defining replicator phases (e.g., Approval, Signing), and describing how many tasks each phase should create.</item>
///   <item>Automatically generating the correct child tasks and wiring up their lifecycle (completion, rejection, etc.)</item>
///   <item>Managing the "replicator state" in the workflow context, so state survives across restarts.</item>
/// </list>
/// </para>
/// <para>
/// <b>How it works:</b><br/>
/// 1. When the workflow starts, <see cref="OnWorkflowActivatedAsync"/> uses a "replicator phase builder"
///    (see <see cref="CreateReplicatorPhaseBuilder"/>) to generate a list of phases, each describing how to create its child activities/tasks.
/// 2. Each phase (e.g., Approval) will create the correct number of child items — typically one per approver or signer.
/// 3. As tasks are completed (or rejected, delegated, etc), the replicator infrastructure manages progress, handles transitions,
///    and fires workflow triggers as needed.
/// </para>
/// <para>
/// <b>Why is this complex?</b><br/>
/// The main challenge is that in real business workflows, the number and assignment of tasks is dynamic: it can change based on runtime data,
/// delegation, rejection, or modification by the user. Replicator patterns allow you to describe "how many" tasks to create for a given phase,
/// and provide the extensibility points for custom workflow logic.
/// </para>
/// <para>
/// <b>Key methods to look at:</b><br/>
/// - <see cref="CreateReplicatorPhaseBuilder"/>: Provides the phase generation config for this workflow type.
/// - <see cref="OnWorkflowActivatedAsync"/>: Kicks off phase generation and initializes replicator state.
/// - <see cref="ActivityProvider"/>: Responsible for creating the actual child activity instance for each task.
/// </para>
/// <para>
/// <b>Tip for implementers:</b><br/>
/// You should not need to directly manipulate replicator state or items in most workflows. Instead, override the builder/provider methods to
/// control phase/task creation, and handle transitions via workflow triggers and the activity provider.
/// </para>
/// </summary>
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
    
    protected override async Task OnWorkflowActivatedAsync(CancellationToken cancellationToken)
    {
        // Ensure SIDs are resolved to PrincipalIds before building phases
        await base.OnWorkflowActivatedAsync(cancellationToken);

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

        InitializeReplicatorManagerAsync(cancellationToken);
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

    protected async Task RunReplicatorAsync(string phaseName, CancellationToken cancellationToken)
    {
        InitializeReplicatorManagerAsync(cancellationToken);
        await _replicatorManager!.RunReplicatorAsync(phaseName, cancellationToken);
    }

    private void InitializeReplicatorManagerAsync(CancellationToken ct)
    {
        if (_replicatorManager == null)
        {
            _replicatorManager = new ReplicatorManager();
            _replicatorManager.LoadReplicatorsAsync(
                WorkflowContext.ReplicatorStates,
                createActivity: item => CreateActivityForReplicator(item.ActivityContext),
                onChildInitialized: OnChildInitialized,
                onAllTasksCompleted: () => TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, ct),
                ct: ct);
        }
    }
}