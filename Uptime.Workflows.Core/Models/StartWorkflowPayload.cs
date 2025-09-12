using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Models;

public class StartWorkflowPayload : IWorkflowPayload
{
    public required string PrincipalSid { get; set; }
    public required Guid WorkflowBaseId { get; set; }
    public required DocumentId DocumentId { get; set; }
    public required WorkflowTemplateId WorkflowTemplateId { get; set; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}