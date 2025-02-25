namespace Uptime.Shared.Models.Workflows;

public record StartWorkflowRequest
{
    public required string Originator { get; init; }
    public required int DocumentId { get; init; }
    public required int WorkflowTemplateId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}