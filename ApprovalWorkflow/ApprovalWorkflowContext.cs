using System.Text.Json;
using Uptime.Workflows.Core;

namespace ApprovalWorkflow;

/// <summary>
/// Workflow context class for ApprovalWorkflow — stores all runtime data required for a single approval workflow instance.
///
/// <para>
/// <b>What is this?</b><br/>
/// <c>ApprovalWorkflowContext</c> extends <see cref="BaseWorkflowContext"/> to add approval-specific fields:
/// <list type="bullet">
///   <item><see cref="AnyTaskRejected"/>: Indicates if any task in the approval process was rejected.</item>
///   <item><see cref="ReplicatorStates"/>: Stores the state of each replicator phase, including all active/inactive child tasks for each phase.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>How is it used?</b><br/>
/// - Each ApprovalWorkflow instance has its own <c>ApprovalWorkflowContext</c>.
/// - All data is persisted as JSON; on load, custom fields are restored using the <see cref="Deserialize"/> override.
/// - The workflow engine uses this context to track all progress, outcomes, and child task states throughout the approval/signing process.
/// </para>
///
/// <para>
/// <b>When to add fields here?</b><br/>
/// - Whenever you need to track extra per-workflow-instance state that is *specific* to ApprovalWorkflow (not generic to all workflows).
///   E.g., flags, lists, current user assignments, special approval/rejection logic.
/// </para>
///
/// <para>
/// <b>Typical usage pattern:</b><br/>
/// <code>
/// if (approvalContext.AnyTaskRejected)
/// {
///     // Custom logic if any approval was rejected
/// }
///
/// // Access or update child task/replicator phase data:
/// approvalContext.ReplicatorStates[phaseId].Items.Add(new ReplicatorItem(...));
/// </code>
/// </para>
///
/// <para>
/// <b>Why is this separate from BaseWorkflowContext?</b><br/>
/// To keep workflow-specific logic/data isolated from core workflow engine data. Each workflow type should define its own context if it needs custom fields.
/// </para>
/// </summary>
public sealed class ApprovalWorkflowContext : BaseWorkflowContext, IReplicatorWorkflowContext
{
    public bool AnyTaskRejected { get; set; }
    public Dictionary<string, ReplicatorState> ReplicatorStates { get; set; } = new();
    
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<ApprovalWorkflowContext>(json);
        if (obj != null)
        {
            AnyTaskRejected = obj.AnyTaskRejected;
            Storage = obj.Storage;
            ReplicatorStates = obj.ReplicatorStates;
        }
    }
}