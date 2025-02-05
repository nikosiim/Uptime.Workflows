using Uptime.Application.Interfaces;

namespace Uptime.Application.Models.Common;

public class WorkflowPayload : IWorkflowPayload
{
    public required string Originator { get; set; }
    public required int DocumentId { get; set; }
    public required int WorkflowTemplateId { get; set; }
}