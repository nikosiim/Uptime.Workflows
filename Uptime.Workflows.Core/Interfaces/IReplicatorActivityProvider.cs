namespace Uptime.Workflows.Core.Interfaces;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(WorkflowTaskContext context, object data);
    void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity);
    void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext);
}