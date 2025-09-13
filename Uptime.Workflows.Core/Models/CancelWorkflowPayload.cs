using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record CancelWorkflowPayload: IUserActionPayload
{
    public required Principal ExecutedBy { get; init; }
    public string? Comment { get; init; }
}