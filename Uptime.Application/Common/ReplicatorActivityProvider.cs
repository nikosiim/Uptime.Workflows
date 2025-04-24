using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Application.Common;

public abstract class ReplicatorActivityProvider(IWorkflowRepository repository) : IReplicatorActivityProvider
{
    protected readonly IWorkflowRepository Repository = repository;

    public abstract IWorkflowActivity CreateActivity(WorkflowTaskContext context, object data);

    public virtual void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity)
    {
    }

    public virtual void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext)
    {
    }
}