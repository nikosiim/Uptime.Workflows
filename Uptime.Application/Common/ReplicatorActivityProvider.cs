using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public abstract class ReplicatorActivityProvider(IWorkflowRepository repository) : IReplicatorActivityProvider
{
    protected readonly IWorkflowRepository Repository = repository;

    public abstract IWorkflowActivity CreateActivity(string phaseName, object data, WorkflowTaskContext context);

    public virtual void OnChildInitialized(string phaseName, object data, IWorkflowActivity activity)
    {
    }

    public virtual void OnChildCompleted<TContext>(string phaseName, UserTaskActivity activity, TContext workflowContext)
    {
    }
}