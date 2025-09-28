using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public record ModificationPayload : IUserActionPayload
{
    public required PrincipalSid ExecutorSid { get; init; }
    public string? ModificationContext { get; init; }
}