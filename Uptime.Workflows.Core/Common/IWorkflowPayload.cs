namespace Uptime.Workflows.Core.Common;

public interface IWorkflowPayload
{
    Guid WorkflowBaseId { get; set; }
    string Originator { get; set; }
    DocumentId DocumentId { get; set; }
    WorkflowTemplateId WorkflowTemplateId { get; set; }
    Dictionary<string, string?> Storage { get; init; }
}