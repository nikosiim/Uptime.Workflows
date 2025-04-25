using System.Text.Json;
using System.Text.Json.Serialization;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public class WorkflowContext : IWorkflowContext
{
    [JsonIgnore] 
    public WorkflowOutcome Outcome { get; set; } = WorkflowOutcome.None;
    public Dictionary<string, string?> Storage { get; set; } = new();

    public virtual string Serialize() => JsonSerializer.Serialize(this);

    public virtual void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<WorkflowContext>(json);
        if (obj != null)
        {
            Outcome = obj.Outcome;
            Storage = obj.Storage;
        }
    }
}