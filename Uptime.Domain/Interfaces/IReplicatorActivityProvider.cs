using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(string phaseName, object data, WorkflowTaskContext context);
    void OnChildInitialized(string phaseName, object data, IWorkflowActivity activity);
    void OnChildCompleted<TContext>(string phaseName, UserTaskActivity activity, TContext workflowContext);
}