using System.Text.Json;
using Uptime.Application.Common;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowContext : WorkflowContext, IReplicatorWorkflowContext
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