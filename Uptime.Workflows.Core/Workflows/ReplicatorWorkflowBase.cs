using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Core;

/* ReplicatorWorkflowBase<TContext>
  
   Base class for workflows that include one or more "replicator" phases.
   A replicator phase fans out work to multiple parallel child activities
   (e.g., sending an approval task to each approver). The replicator tracks
   the state of all child items and signals the workflow when the phase is
   complete.
  
   Key concepts:
     • ReplicatorState  – runtime state of one phase, including all items.
     • ReplicatorItem   – a single pending/active/completed child.
     • ReplicatorManager– orchestrates execution of replicator phases.
     • Child activity   – the actual workflow activity run for each item.
  
   Hook-based design:
     Unlike the previous provider-based model, workflows now override virtual
     methods directly instead of supplying an IReplicatorActivityProvider.
  
     Override points:
       CreateChildActivity(ctx)    – create a concrete IUserTaskActivity for a
                                     given item. Used in AlterTask path.
       CreateActivityForReplicator(ctx)
                                   – create the IWorkflowActivity instance for
                                     the replicator engine. Default maps to
                                     CreateChildActivity.
       OnChildInitialized(phaseId, ctx, act)
                                   – invoked when a new child activity has been
                                     created and initialized.
       OnChildCompleted(phaseId, act)
                                   – invoked when a user task activity is marked
                                     completed (including reject/delegate cases).
  
   AlterTask path:
     OnTaskAlteredAsync(...) calls CreateChildActivity to reconstruct the
     activity for the given task, invokes ChangedTaskAsync, and then calls
     OnChildCompleted + RunReplicatorAsync if the task finished.
  
   Usage pattern:
     • Derive your workflow from ReplicatorWorkflowBase<TContext>.
     • Override the hooks to decide what activity to run per phase, and to
       handle special logic when a task completes (delegation, rejection, etc.).
     • Configure your state machine to call RunReplicatorAsync(...) on entry
       to each replicator phase.
*/
public abstract class ReplicatorWorkflowBase<TContext>(
    IWorkflowService workflowService,
    ITaskService taskService,
    IHistoryService historyService,
    IPrincipalResolver principalResolver,
    IWorkflowOutboundNotifier? notifier,
    ILogger<WorkflowBase<TContext>> logger)
    : ActivityWorkflowBase<TContext>(workflowService, taskService, historyService, notifier, logger)
    where TContext : class, IReplicatorWorkflowContext, new()
{
    #region Fields & Properties

    private ReplicatorManager? _replicatorManager;
    private readonly ILogger<WorkflowBase<TContext>> _logger = logger;

    // Buffer for batching "tasks created" in Parallel phases
    private readonly Dictionary<string, List<TaskProjection>> _createdBuffer = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Public API

    // None 

    #endregion

    #region Protected Hooks

    protected override async Task OnWorkflowActivatedAsync(CancellationToken ct)
    {
        await base.OnWorkflowActivatedAsync(ct);

        List<ReplicatorPhase> phases = CreateAndStoreReplicatorStates();
        await PreloadParallelPhasesPrincipalsAsync(phases, ct);

        InitializeReplicatorManager(ct);
    }
    protected override async Task OnTaskAlteredAsync(WorkflowEventType action, IWorkflowActivityContext activityContext, PrincipalSid executorSid, Dictionary<string, string?> inputData, CancellationToken ct)
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
            _logger.LogWarning("Workflow {WorkflowId} update not allowed in this phase. Phase={Phase}", WorkflowId, Machine.State.Value);
        }

        return modificationContext;
    }

    #endregion

    #region Replicator hooks

    protected virtual IUserTaskActivity? CreateChildActivity(IWorkflowActivityContext ctx) => null;
    protected virtual IWorkflowActivity CreateActivityForReplicator(IWorkflowActivityContext ctx)
    {
        IUserTaskActivity? userTask = CreateChildActivity(ctx);
        return userTask ?? throw new InvalidOperationException("Phase requires an activity but CreateChildActivity returned null.");
    }
    protected virtual void OnChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity) { }
    protected virtual void OnChildCompleted(string phaseId, IUserTaskActivity activity) { }

    /// <summary>
    /// Selects which principals to preload for a phase prior to execution.
    /// </summary>
    /// <remarks>
    /// Default returns the assignees of the phase's items. Override if your activity needs
    /// additional principals (e.g., watchers, CC recipients) to be resolved before start.
    /// Used by <see cref="EnsurePhasePrincipalsPreloadedAsync"/>.
    /// </remarks>
    protected virtual IEnumerable<PrincipalSid> GetPhaseSidsForPreload(string phaseName, ReplicatorState state)
    {
        return state.Items.Select(i => i.ActivityContext.AssignedToSid).ToList();
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
        var cfg = WorkflowDefinition.ReplicatorConfiguration?.PhaseConfigurations
                  ?? throw new InvalidOperationException($"{GetType().Name} requires ReplicatorConfiguration.");
        return new ReplicatorPhaseBuilder(cfg);
    }

    // Primary, flexible API — override if you want to change who/what is returned.
    protected virtual async Task<IReadOnlyList<Principal>> GetPhaseParticipantsAsync(string phaseId, CancellationToken ct)
    {
        if (!WorkflowContext.ReplicatorStates.TryGetValue(phaseId, out ReplicatorState? state))
            return [];

        List<PrincipalSid> sids = state.Items.Select(i => i.ActivityContext.AssignedToSid).Distinct().ToList();
        if (sids.Count > 0)
            await principalResolver.EnsurePrincipalsCachedAsync(sids, ct);

        var principals = new List<Principal>(capacity: sids.Count);
        foreach (PrincipalSid sid in sids)
        {
            Principal? p = await principalResolver.TryResolveBySidAsync(sid, ct);
            if (p != null) principals.Add(p);
        }

        return principals;
    }
    protected virtual async Task DispatchTasksCreatedPayloadAsync(string phaseName, bool isParallel, List<TaskProjection> tasks, CancellationToken ct)
    {
        if (Notifier is null) return;

        IOutboundNotificationPayload? payload = await BuildTasksCreatedPayloadAsync(phaseName, tasks, ct);
        if (payload is null) return;

        await Notifier.NotifyAsync(WorkflowEvents.WorkflowTasksCreated, payload, ct);
    }
    protected virtual bool ShouldNotifyOnTasksCreated(string phaseId) => false;

    #endregion

    #region Protected Internals
    protected List<ReplicatorPhase> CreateAndStoreReplicatorStates()
    {
        IReplicatorPhaseBuilder builder = CreateReplicatorPhaseBuilder();
        List<ReplicatorPhase> replicatorPhases = builder.BuildPhases(WorkflowContext);

        WorkflowContext.ReplicatorStates = replicatorPhases.ToDictionary(
            phase => phase.PhaseName,
            phase => new ReplicatorState
            {
                ReplicatorType = phase.Type,
                Items = phase.TaskContext.Select(ac => new ReplicatorItem(ac)).ToList()
            });

        return replicatorPhases;
    }

    /// <summary>
    /// Bulk warm-up for principal cache across all phases configured as Parallel.
    /// Called once during workflow activation after phases are built. Collects every
    /// assignee SID from Parallel phases and preloads them to avoid N+1 lookups and
    /// reduce first-run latency when those phases start.
    /// </summary>
    protected async Task PreloadParallelPhasesPrincipalsAsync(IEnumerable<ReplicatorPhase> phases, CancellationToken ct)
    {
        // Only parallel phases need an up-front warm-up; sequential phases are warmed as needed.
        IEnumerable<PrincipalSid> sids = phases
            .Where(p => p.Type == ReplicatorType.Parallel)
            .SelectMany(p => p.TaskContext.Select(c => c.AssignedToSid))
            .Distinct()
            .ToList();

        if (sids.Any())
            await principalResolver.EnsurePrincipalsCachedAsync(sids, ct);
    }
    private void InitializeReplicatorManager(CancellationToken ct)
    {
        if (_replicatorManager == null)
        {
            _replicatorManager = new ReplicatorManager();

            _replicatorManager.LoadReplicators(
                WorkflowContext.ReplicatorStates,
                createActivity: item => CreateActivityForReplicator(item.ActivityContext),
                onChildInitialized: HandleChildInitialized,
                onAllTasksCompleted: () => TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, ct));
        }
    }
    protected async Task RunReplicatorAsync(string phaseName, CancellationToken ct)
    {
        InitializeReplicatorManager(ct);

        ResetPhaseNotificationBuffer(phaseName);

        await EnsurePhasePrincipalsPreloadedAsync(phaseName, ct);
        await _replicatorManager!.RunReplicatorAsync(phaseName, ct);
        await DispatchTaskCreatedNotificationsAsync(phaseName, ct);
    }

    /// <summary>
    /// Ensures principals for the specified phase are preloaded right before execution.
    /// If the phase is Parallel, gathers SIDs (via <see cref="GetPhaseSidsForPreload"/>) and
    /// preloads them. This guards against items added or changed after activation.
    /// </summary>
    protected async Task EnsurePhasePrincipalsPreloadedAsync(string phaseName, CancellationToken ct)
    {
        if (WorkflowContext.ReplicatorStates.TryGetValue(phaseName, out ReplicatorState? state)
            && state.ReplicatorType == ReplicatorType.Parallel)
        {
            IEnumerable<PrincipalSid> sids = GetPhaseSidsForPreload(phaseName, state);
            List<PrincipalSid> list = sids.Distinct().ToList();
            if (list.Count > 0)
            {
                await principalResolver.EnsurePrincipalsCachedAsync(list, ct);
            }
        }
    }
    protected async Task DispatchTaskCreatedNotificationsAsync(string phaseName, CancellationToken ct)
    {
        if (Notifier == null)
            return;

        if (!WorkflowContext.ReplicatorStates.TryGetValue(phaseName, out ReplicatorState? state))
            return;

        if (!_createdBuffer.TryGetValue(phaseName, out List<TaskProjection>? created) || created.Count == 0)
            return;

        if (state.ReplicatorType == ReplicatorType.Parallel)
        {
            await DispatchTasksCreatedPayloadAsync(phaseName, true, created, ct);
        }
        else
        {
            foreach (TaskProjection one in created)
            {
                await DispatchTasksCreatedPayloadAsync(phaseName, false, [one], ct);
            }
        }

        ResetPhaseNotificationBuffer(phaseName);
    }
    protected void UpdateWorkflowContextReplicatorState(Guid taskGuid, ReplicatorItemStatus status)
    {
        ReplicatorItem? item = WorkflowContext.ReplicatorStates.FindReplicatorItem(taskGuid);
        if (item != null)
        {
            item.Status = status;
        }
    }
    protected Task<string> GetParticipantNamesAsync(string phaseId, CancellationToken ct)
    {
        return GetParticipantNamesCoreAsync(phaseId, ct);

        async Task<string> GetParticipantNamesCoreAsync(string id, CancellationToken token)
        {
            IReadOnlyList<Principal> people = await GetPhaseParticipantsAsync(id, token);
            return string.Join(", ", people.Select(p => p.Name));
        }
    }

    #endregion

    #region Private Internals

    private void ResetPhaseNotificationBuffer(string phaseName)
    {
        _createdBuffer.Remove(phaseName);
    }
    private void HandleChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity)
    {
        // Wraps child init: track created items for notifier, then call the workflow hook.

        if (ShouldNotifyOnTasksCreated(phaseId))
        {
            if (!_createdBuffer.TryGetValue(phaseId, out List<TaskProjection>? list))
            {
                list = [];
                _createdBuffer[phaseId] = list;
            }

            list.Add(new TaskProjection(context.TaskGuid, phaseId, context.AssignedToSid));
        }


        OnChildInitialized(phaseId, context, activity);
    }
    
    #endregion
}