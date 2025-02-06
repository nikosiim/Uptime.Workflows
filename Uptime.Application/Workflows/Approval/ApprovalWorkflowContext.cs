using Uptime.Application.Common;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowContext : IWorkflowContext
{
    public bool AnyTaskRejected { get; set; }
    public Dictionary<string, object?> Storage { get; set; } = new();
    public ReplicatorState<ApprovalTaskContext> ReplicatorState { get; set; } = new();
}