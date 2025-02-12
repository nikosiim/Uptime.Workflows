using Uptime.Application.Common;

namespace Uptime.Application.Interfaces;

public interface IActivityWorkflowMachine : IWorkflowMachine
{
    Task AlterTaskCoreAsync(WorkflowTaskContext context);
}
