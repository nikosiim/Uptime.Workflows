using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowActivityContext
{
    Guid TaskGuid { get; }
    string? PhaseId { get; }
    PrincipalSid AssignedToSid { get; }
    DateTimeOffset? DueDate { get; }
    string? Description { get; }
    Dictionary<string, string?> Storage { get; }
}