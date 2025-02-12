using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowPayload
{
    string Originator { get; set; }
    DocumentId DocumentId { get; set; }
    WorkflowTemplateId WorkflowTemplateId { get; set; }
    Dictionary<string, string?> Storage { get; init; }
}