using System.Text.Json;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class WorkflowTaskContext : IWorkflowTask
{
    public WorkflowId WorkflowId { get; }
    public string? PhaseId { get; set; }
    public Guid TaskGuid { get; set;  }
    public TaskId TaskId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public WorkflowTaskStatus TaskStatus { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();

    public WorkflowTaskContext(WorkflowId workflowId, string? phaseId = null)
    {
        WorkflowId = workflowId;
        PhaseId = phaseId;
    }

    public WorkflowTaskContext(WorkflowTask workflowTask)
    {
        WorkflowId = (WorkflowId)workflowTask.WorkflowId;
        TaskId = (TaskId)workflowTask.Id;
        TaskGuid = workflowTask.TaskGuid;
        AssignedTo = workflowTask.AssignedTo;
        AssignedBy = workflowTask.AssignedBy;
        TaskDescription = workflowTask.Description;
        DueDate = workflowTask.DueDate;
        PhaseId = workflowTask.PhaseId;

        Storage = string.IsNullOrWhiteSpace(workflowTask.StorageJson)
            ? new Dictionary<string, string?>()
            : JsonSerializer.Deserialize<Dictionary<string, string?>>(workflowTask.StorageJson) ?? new Dictionary<string, string?>();
    }
}