using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record AlterTaskPayload : IUserActionPayload
{
    public required Principal ExecutedBy { get; init; }
    public required WorkflowTaskContext Context { get; init; }
    public required Dictionary<string, string?> InputData { get; init; }
}