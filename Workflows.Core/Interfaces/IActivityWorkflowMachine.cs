using Workflows.Core.Common;
using Workflows.Core.Enums;
using Workflows.Core.Models;

namespace Workflows.Core.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task<Result<Unit>> AlterTaskAsync(WorkflowEventType action, UpdateTaskPayload payload, CancellationToken ct);
}