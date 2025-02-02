using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Common;

namespace Uptime.Application.Models.Approval;

public sealed class ApprovalTaskContext : IWorkflowTask, IWorkflowItem
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public Dictionary<string, object?> Storage { get; } = new();

    /* Context Properties */
    public bool IsCompleted { get; set; }
    public TaskOutcome Outcome { get; set; }

    public ApprovalTaskContext()
    {

    }

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
        Outcome = source.Outcome;
    }
}