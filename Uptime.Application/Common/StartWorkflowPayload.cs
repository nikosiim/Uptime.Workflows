using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Application.Common;

public class StartWorkflowPayload : IWorkflowPayload
{
    public required Guid WorkflowBaseId { get; set; }
    public required string Originator { get; set; }
    public required DocumentId DocumentId { get; set; }
    public required WorkflowTemplateId WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}