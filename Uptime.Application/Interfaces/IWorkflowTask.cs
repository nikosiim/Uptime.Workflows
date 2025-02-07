using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IWorkflowTask
{
    TaskId TaskId { get; }
    WorkflowId WorkflowId { get; }
    string AssignedTo { get; }
    string AssignedBy { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
    Dictionary<string, string?> Storage { get; }
}