namespace Uptime.Client.Contracts;

public record WorkflowTemplateResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string WorkflowBaseId { get; init; }
    public string? AssociationDataJson { get; init; }
    public DateTimeOffset Created { get; init; }
}