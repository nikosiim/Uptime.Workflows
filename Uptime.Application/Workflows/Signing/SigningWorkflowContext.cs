using System.Text.Json;
using Uptime.Application.Common;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflowContext : WorkflowContext
{
    public SigningTaskData? SigningTask { get; set; }
    
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<SigningWorkflowContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}