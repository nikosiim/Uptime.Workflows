using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowPayload
{
    string PrincipalSid { get; set; }
    Guid WorkflowBaseId { get; set; }
    DocumentId DocumentId { get; set; }
    WorkflowTemplateId WorkflowTemplateId { get; set; }
    Dictionary<string, string?> Storage { get; init; }
}