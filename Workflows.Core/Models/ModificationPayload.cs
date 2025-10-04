using Workflows.Core.Common;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Models;

public record ModificationPayload : IUserActionPayload
{
    public required PrincipalSid ExecutorSid { get; init; }
    public string? ModificationContext { get; init; }
}