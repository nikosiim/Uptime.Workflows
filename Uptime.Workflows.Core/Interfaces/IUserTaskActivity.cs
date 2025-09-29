using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Interfaces;

public interface IUserTaskActivity : IWorkflowActivity
{
    Task ChangedTaskAsync(WorkflowEventType action, IWorkflowActivityContext context,
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct);
}