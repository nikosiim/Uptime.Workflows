using Uptime.Application.Common;

namespace Uptime.Application.Models.Approval;

public sealed class ApprovalTaskContext : WorkflowTaskBase
{
    public ApprovalTaskContext() { }

    public ApprovalTaskContext(ApprovalTaskContext source)
    {
        Id = source.Id;
        WorkflowId = source.WorkflowId;
        AssignedBy = source.AssignedBy;
        AssignedTo = source.AssignedTo;
        TaskDescription = source.TaskDescription;
        DueDate = source.DueDate;
        Storage = new Dictionary<string, object?>(source.Storage);
        IsCompleted = source.IsCompleted;
    }
}