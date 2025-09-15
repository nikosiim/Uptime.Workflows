using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public abstract class ReplicatorActivityProvider : IReplicatorActivityProvider
{
    public abstract IWorkflowActivity CreateActivity(IWorkflowTaskContext context);

    public virtual void OnChildInitialized(string phaseId, IWorkflowTaskContext context, IWorkflowActivity activity)
    {
    }
    
    public virtual void OnChildCompleted(string phaseId, IUserTaskActivity activity, Principal executedBy)
    {
    }
}