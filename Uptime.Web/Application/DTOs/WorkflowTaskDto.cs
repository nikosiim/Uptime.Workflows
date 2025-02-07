using Uptime.Shared;
using Uptime.Shared.Common;
using Uptime.Shared.Enums;

namespace Uptime.Web.Application.DTOs;

public record WorkflowTaskDto
{
    public int Id { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public WorkflowTaskStatus Status { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; init; }
    public string? Document { get; init; }
    public int WorkflowId { get; init; }
    public string? Title => JsonHelper.ExtractValue(StorageJson, GlobalConstants.TaskStorageKeys.TaskTitle);
}