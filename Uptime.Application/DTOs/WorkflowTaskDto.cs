using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;

public record WorkflowTaskDto
{
    public int Id { get; init; }
    public Guid TaskGuid { get; init; }
    public string? AssignedTo { get; init; }
    public string? AssignedBy { get; init; }
    public WorkflowTaskStatus Status { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? StorageJson { get; set; }
    public string? Document { get; set; }
    public int WorkflowId { get; set; }
}