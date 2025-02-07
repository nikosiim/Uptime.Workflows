namespace Uptime.Application.Interfaces;

public interface IWorkflowPayload
{
    string Originator { get; set; }
    int DocumentId { get; set; }
    int WorkflowTemplateId { get; set; }
    Dictionary<string, string?> Storage { get; init; }
}