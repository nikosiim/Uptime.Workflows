using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowPayload : IWorkflowPayload
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
    public Dictionary<string, object> Data { get; init; } = new();
}