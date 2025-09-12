using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Models;

public sealed class AlterTaskPayload
{
    public required PrincipalId ExecutedByPrincipalId { get; set; }
    public required WorkflowTaskContext Context { get; set; }
    public required Dictionary<string, string?> InputData { get; set; }
}