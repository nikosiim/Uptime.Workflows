namespace Uptime.Workflows.Api.Contracts;

public record LibraryWorkflowTemplateResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string WorkflowBaseId { get; set; } = null!;
    public string? AssociationDataJson { get; set; }
    public DateTimeOffset Created { get; set; }
}