namespace Workflows.Api.Contracts;

public record WorkflowTemplateResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string WorkflowBaseId { get; init; } = null!;
    public string? AssociationDataJson { get; init; }
    public DateTimeOffset Created { get; init; }
}