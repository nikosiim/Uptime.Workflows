namespace Uptime.Domain.Interfaces;

public interface IUserTaskActivityData : IWorkflowTaskData
{
    string AssignedTo { get; }
    string AssignedBy { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
}