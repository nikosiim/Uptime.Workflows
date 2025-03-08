using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(string phaseId, object data, WorkflowTaskContext context);
    void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity);
    void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext);
}