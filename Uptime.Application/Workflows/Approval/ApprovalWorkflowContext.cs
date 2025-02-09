using System.Text.Json;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalWorkflowContext : IReplicatorWorkflowContext<ApprovalTaskData>
{
    public bool AnyTaskRejected { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();
    public Dictionary<string, ReplicatorState<ApprovalTaskData>> ReplicatorStates { get; set; } = new();

    public string Serialize() => JsonSerializer.Serialize(this);

    public void Deserialize(string json)
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