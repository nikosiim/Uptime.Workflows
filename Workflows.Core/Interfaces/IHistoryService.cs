using Workflows.Core.Common;
using Workflows.Core.Enums;

namespace Workflows.Core.Interfaces;

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