namespace Uptime.Client.Contracts;

public record WorkflowTemplateUpdateRequest
{
    public required string ExecutorSid { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public string? AssociationDataJson { get; init; }
}