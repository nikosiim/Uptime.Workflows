using Uptime.Client.Application.Common;
using Uptime.Client.Contracts;

namespace Uptime.Client.Application.DTOs;

public record WorkflowTaskData
{
    public required Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int WorkflowId { get; init; }
    public string? DisplayStatus { get; init; }
    public WorkflowTaskStatus InternalStatus { get; init; }

    public string? Title => StorageJson.GetValue(Constants.StorageKeys.Activity.TaskTitle);
}