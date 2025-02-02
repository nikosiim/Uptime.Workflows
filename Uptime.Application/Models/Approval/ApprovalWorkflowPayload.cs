namespace Uptime.Application.Models.Approval;

public record ApprovalWorkflowPayload
{
    public required int WorkflowTemplateId { get; set; }
    public required int DocumentId { get; set; }
    public required string Originator { get; set; }
    public required List<string> Executors { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}