using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Interfaces;

public interface IHistoryService
{
    Task CreateAsync(
        WorkflowId workflowId,
        WorkflowEventType eventType,
        PrincipalSid principalSid,
        string? description,
        string? comment = null,
        CancellationToken ct = default);
}