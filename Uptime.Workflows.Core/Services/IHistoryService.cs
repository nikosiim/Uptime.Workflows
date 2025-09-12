using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Services;

public interface IHistoryService
{
    Task CreateAsync(
        WorkflowId workflowId,
        WorkflowEventType eventType,
        PrincipalId principalId,
        string? description,
        string? comment = null,
        CancellationToken cancellationToken = default);
}