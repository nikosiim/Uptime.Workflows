using Workflows.Core.Common;
using Workflows.Core.Enums;

namespace Workflows.Core.Interfaces;

public interface IUserTaskActivity : IWorkflowActivity
{
    Task ChangedTaskAsync(WorkflowEventType action, IWorkflowActivityContext context,
        PrincipalSid executorSid, Dictionary<string, string?> payload, CancellationToken ct);
}