using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core;

public interface IUserTaskActivityData : IWorkflowTaskData
{
    PrincipalId AssignedToPrincipalId { get; }
    PrincipalId AssignedByPrincipalId { get; }
    string? TaskDescription { get; }
    DateTime? DueDate { get; }
}