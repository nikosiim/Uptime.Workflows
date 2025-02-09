namespace Uptime.Application.Interfaces;

public interface IUserTaskActivityData
{
    string AssignedTo { get; }
    string AssignedBy { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
}