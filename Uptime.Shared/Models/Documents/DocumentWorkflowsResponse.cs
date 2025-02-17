namespace Uptime.Shared.Models.Documents;

public record DocumentWorkflowsResponse
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
    public string? WorkflowTemplateName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Outcome { get; init; }
    public bool IsActive { get; init; }
}