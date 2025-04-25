using Uptime.Workflows.Core.Interfaces;

namespace Workflow.Minimized.Stateless;

public class StatelessStateMachineFactory<TState, TTrigger> : IStateMachineFactory<TState, TTrigger>
{
    public IStateMachine<TState, TTrigger> Create(TState initialState)
    {
        return new StatelessStateMachineAdapter<TState, TTrigger>(initialState);
    }
}