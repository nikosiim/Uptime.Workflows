using Uptime.Application.Common;

namespace Uptime.Application.Workflows.Approval;

public sealed class ApprovalTaskContext : WorkflowTaskContext
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
        Storage = new Dictionary<string, string?>(source.Storage);
        IsCompleted = source.IsCompleted;
    }
}