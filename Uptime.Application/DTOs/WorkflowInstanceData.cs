using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;
public record WorkflowInstanceData
{
    public WorkflowPhase Phase { get; init; }
    public string? InstanceDataJson { get; init; }
}