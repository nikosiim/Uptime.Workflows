using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

/// <summary>
/// Provides a base implementation of <see cref="IWorkflowActivityFactory{TData}"/> with default behavior.
/// Derived classes can override methods like OnChildInitialized and OnChildCompleted if they need custom logic.
/// </summary>
/// <typeparam name="TData">The type of task data.</typeparam>
public abstract class BaseWorkflowActivityFactory<TData>(IWorkflowRepository repository) : IWorkflowActivityFactory<TData>
{
    protected readonly IWorkflowRepository Repository = repository;

    public abstract IWorkflowActivity CreateActivity(TData data, WorkflowTaskContext context);

    public virtual void OnChildInitialized(string phaseName, TData data, IWorkflowActivity activity)
    {
        Console.WriteLine($"Child activity initialized for phase {phaseName}.");
    }

    public virtual void OnChildCompleted(string phaseName, TData data, IWorkflowActivity activity)
    {
        Console.WriteLine($"Child activity completed for phase {phaseName}.");
    }
}