using Uptime.Domain.Interfaces;

namespace Uptime.Application.Stateless;

public class StatelessStateMachineFactory<TState, TTrigger> : IStateMachineFactory<TState, TTrigger>
{
    public IStateMachine<TState, TTrigger> Create(TState initialState)
    {
        return new StatelessStateMachineAdapter<TState, TTrigger>(initialState);
    }
}