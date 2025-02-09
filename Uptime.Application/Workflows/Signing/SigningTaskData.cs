using Uptime.Application.Interfaces;

namespace Uptime.Application.Workflows.Signing;

public class SigningTaskData : IUserTaskActivityData
{
    public required string AssignedTo { get; set; }
    public required string AssignedBy { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
}