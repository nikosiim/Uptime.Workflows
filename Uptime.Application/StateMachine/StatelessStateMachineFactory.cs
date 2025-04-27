using Uptime.Workflows.Core;

namespace Uptime.Application.StateMachine;

public class StatelessStateMachineFactory<TState, TTrigger> : IStateMachineFactory<TState, TTrigger>
{
    public IStateMachine<TState, TTrigger> Create(TState initialState)
    {
        return new StatelessStateMachineAdapter<TState, TTrigger>(initialState);
    }
}