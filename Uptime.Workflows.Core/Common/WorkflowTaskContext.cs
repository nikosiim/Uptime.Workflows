using System.Text.Json;
using Uptime.Workflows.Core.Entities;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Common;

public class WorkflowTaskContext(WorkflowId workflowId, Guid taskGuid, string? phaseId = null)
    : IWorkflowTask
{
    public WorkflowId WorkflowId { get; } = workflowId;
    public Guid TaskGuid { get; } = taskGuid;
    public string? PhaseId { get; set; } = phaseId;
    public TaskId TaskId { get; set; }
    public string AssignedTo { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public WorkflowTaskStatus TaskStatus { get; set; }
    public Dictionary<string, string?> Storage { get; set; } = new();

    public WorkflowTaskContext(WorkflowTask workflowTask) : this((WorkflowId)workflowTask.WorkflowId, workflowTask.TaskGuid, workflowTask.PhaseId)
    {
        TaskId = (TaskId)workflowTask.Id;
        AssignedTo = workflowTask.AssignedTo;
        AssignedBy = workflowTask.AssignedBy;
        TaskDescription = workflowTask.Description;
        DueDate = workflowTask.DueDate;

        Storage = string.IsNullOrWhiteSpace(workflowTask.StorageJson)
            ? new Dictionary<string, string?>()
            : JsonSerializer.Deserialize<Dictionary<string, string?>>(workflowTask.StorageJson) ?? new Dictionary<string, string?>();
    }
}