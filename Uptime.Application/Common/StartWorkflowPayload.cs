using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class StartWorkflowPayload : IWorkflowPayload
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}