using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public static class StateMachineFactory
{
    public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState)
    {
        return new StatelessStateMachineAdapter<TState, TTrigger>(initialState);
    }
}