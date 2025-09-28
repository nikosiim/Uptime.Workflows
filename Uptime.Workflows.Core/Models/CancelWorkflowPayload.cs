using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record CancelWorkflowPayload: IUserActionPayload
{
    public required PrincipalSid ExecutorSid { get; init; }
    public string? Comment { get; init; }
}