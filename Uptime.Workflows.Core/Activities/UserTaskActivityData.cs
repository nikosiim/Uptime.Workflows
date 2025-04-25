using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public class UserTaskActivityData : IUserTaskActivityData
{
    public required string AssignedTo { get; set; }
    public required string AssignedBy { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
}