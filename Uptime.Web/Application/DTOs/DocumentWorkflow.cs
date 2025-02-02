using Uptime.Shared.Enums;

namespace Uptime.Web.Application.DTOs;

public record DocumentWorkflow
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
    public string? WorkflowTemplateName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public WorkflowStatus Status { get; init; }
}