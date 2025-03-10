using System.Text.Json;
using Uptime.Application.Common;
using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflowContext : WorkflowContext
{
    public UserTaskActivityData? SigningTask { get; set; }
    
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<SigningWorkflowContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}