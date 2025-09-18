namespace Uptime.Workflows.Api.Contracts;

public record StartWorkflowRequest(string InitiatorSid, int DocumentId, int WorkflowTemplateId)
{
    public Dictionary<string, string?> Storage { get; init; } = new();
}