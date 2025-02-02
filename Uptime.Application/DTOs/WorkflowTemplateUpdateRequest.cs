namespace Uptime.Application.DTOs;

public record WorkflowTemplateUpdateRequest
{
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}