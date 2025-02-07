namespace Uptime.Application.Interfaces;

public interface IWorkflowTask
{
    int Id { get; }
    int WorkflowId { get; }
    string AssignedTo { get; }
    string AssignedBy { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
    Dictionary<string, string?> Storage { get; }
}