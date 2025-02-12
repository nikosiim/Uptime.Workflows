using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task AlterTaskCoreAsync(WorkflowTaskContext context, CancellationToken cancellationToken);
}