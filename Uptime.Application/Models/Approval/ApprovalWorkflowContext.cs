using Uptime.Application.Interfaces;
using Uptime.Application.Models.Common;

namespace Uptime.Application.Models.Approval;

public sealed class ApprovalWorkflowContext : IWorkflowContext
{
    public bool AnyTaskRejected { get; set; }

    public ReplicatorState<ApprovalTaskContext> ReplicatorState { get; set; } = new();
}