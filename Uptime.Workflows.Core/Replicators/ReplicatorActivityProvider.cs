namespace Uptime.Workflows.Core;

public abstract class ReplicatorActivityProvider : IReplicatorActivityProvider
{
    public abstract IWorkflowActivity CreateActivity(WorkflowTaskContext context, object data);

    public virtual void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity)
    {
    }

    public virtual void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext)
    {
    }
}