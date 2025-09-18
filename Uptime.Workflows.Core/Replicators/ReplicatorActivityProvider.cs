using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

public abstract class ReplicatorActivityProvider : IReplicatorActivityProvider
{
    public abstract IWorkflowActivity CreateActivity(IWorkflowActivityContext context);

    public virtual void OnChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity)
    {
    }
    
    public virtual void OnChildCompleted(string phaseId, IUserTaskActivity activity, Principal executedBy)
    {
    }
}