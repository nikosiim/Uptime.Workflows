namespace Uptime.Client.Application.DTOs;

public record DocumentWorkflow
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
    public string? WorkflowTemplateName { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? Outcome { get; init; }
    public bool IsActive { get; init; }
}