using Uptime.Domain.Common;
using Uptime.Shared.Enums;

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
    WorkflowTaskStatus TaskStatus { get; }
    Dictionary<string, string?> Storage { get; }
}