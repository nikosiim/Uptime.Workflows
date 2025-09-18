namespace Uptime.Shared.Models.Workflows;

public record StartWorkflowRequest(string InitiatorSid, int DocumentId, int WorkflowTemplateId)
{
    public Dictionary<string, string?> Storage { get; init; } = new();
}