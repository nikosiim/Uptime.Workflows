using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public abstract class ReplicatorActivityProvider(IWorkflowRepository repository) : IReplicatorActivityProvider
{
    protected readonly IWorkflowRepository Repository = repository;

    public abstract IWorkflowActivity CreateActivity(string phaseId, object data, WorkflowTaskContext context);

    public virtual void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity)
    {
    }

    public virtual void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext)
    {
    }
}