using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public interface IWorkflowTask
{
    TaskId TaskId { get; }
    Guid TaskGuid { get; }
    WorkflowId WorkflowId { get; }
    string? PhaseId { get; }
    PrincipalId AssignedToPrincipalId { get; }
    PrincipalId AssignedByPrincipalId { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
    WorkflowTaskStatus TaskStatus { get; }
    Dictionary<string, string?> Storage { get; }
}