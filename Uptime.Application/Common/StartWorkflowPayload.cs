using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public class StartWorkflowPayload : IWorkflowPayload
{
    public required string Originator { get; set; }
    public required DocumentId DocumentId { get; set; }
    public required WorkflowTemplateId WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}