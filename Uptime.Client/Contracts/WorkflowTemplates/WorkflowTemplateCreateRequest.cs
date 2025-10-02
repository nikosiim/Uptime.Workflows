namespace Uptime.Client.Contracts;

public record WorkflowTemplateCreateRequest
{
    public required string ExecutorSid { get; init; }
    public required string SourceSiteUrl { get; init; }
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required Guid LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
}