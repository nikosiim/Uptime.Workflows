using Microsoft.Extensions.Logging;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Workflows;

public abstract class ActivityWorkflowBase<TContext>(
    IStateMachineFactory<BaseState, WorkflowTrigger> stateMachineFactory,
    IWorkflowRepository repository, 
    ILogger<WorkflowBase<TContext>> logger)
    : WorkflowBase<TContext>(stateMachineFactory, repository, logger: logger)
    where TContext : class, IWorkflowContext, new()
{
    protected override async Task OnTaskAlteredAsync(WorkflowTaskContext context, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        await OnTaskChangedAsync(context, payload, cancellationToken);
        await SaveWorkflowStateAsync(cancellationToken);
    }
    
    protected virtual Task OnTaskChangedAsync(WorkflowTaskContext taskContext, Dictionary<string, string?> payload, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}