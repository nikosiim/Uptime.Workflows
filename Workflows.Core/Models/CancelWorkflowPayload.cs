using Workflows.Core.Common;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Models;

public record CancelWorkflowPayload: IUserActionPayload
{
    public required PrincipalSid ExecutorSid { get; init; }
    public string? Comment { get; init; }
}