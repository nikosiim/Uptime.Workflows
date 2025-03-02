namespace Uptime.Shared.Models.WorkflowTemplates;

public record WorkflowTemplateUpdateRequest
{
    public string TemplateName { get; init; } = null!;
    public string WorkflowName { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
}