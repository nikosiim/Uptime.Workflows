using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core;

public class WorkflowTaskContext : IWorkflowTask
{
    public required WorkflowId WorkflowId { get; set; }
    public required Guid TaskGuid { get; set; }
    public string? PhaseId { get; set; }
    public TaskId TaskId { get; set; }
    public PrincipalId AssignedToPrincipalId { get; set; }
    public PrincipalId AssignedByPrincipalId { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public WorkflowTaskStatus TaskStatus { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();
}