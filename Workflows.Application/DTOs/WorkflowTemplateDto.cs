namespace Workflows.Application.DTOs;

public record WorkflowTemplateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}