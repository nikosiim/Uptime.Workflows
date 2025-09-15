using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core;

public interface IWorkflowTaskContext
{
    Guid TaskGuid { get; }
    string? PhaseId { get; }
    PrincipalId AssignedToPrincipalId { get; }
    PrincipalId AssignedByPrincipalId { get; }
    DateTime? DueDate { get; }
    Dictionary<string, string?> Storage { get; }
}