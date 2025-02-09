using System.Text.Json;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflowContext : IWorkflowContext
{
    public SigningTaskData? SigningTask { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();

    public string Serialize() => JsonSerializer.Serialize(this);

    public void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<SigningWorkflowContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}