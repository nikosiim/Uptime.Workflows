using Uptime.Domain.Interfaces;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalTaskData : IUserTaskActivityData
{
    public required string AssignedTo { get; set; }
    public required string AssignedBy { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }

    public static ApprovalTaskData Copy(IUserTaskActivityData source)
    {
        return new ApprovalTaskData
        {
            AssignedBy = source.AssignedBy,
            AssignedTo = source.AssignedTo,
            TaskDescription = source.TaskDescription,
            DueDate = source.DueDate
        };
    }
}