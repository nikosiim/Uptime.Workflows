using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowTask
{
    TaskId TaskId { get; }
    Guid TaskGuid { get; }
    WorkflowId WorkflowId { get; }
    string AssignedTo { get; }
    string AssignedBy { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
    Dictionary<string, string?> Storage { get; }
}