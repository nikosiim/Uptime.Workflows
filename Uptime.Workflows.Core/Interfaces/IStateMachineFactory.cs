namespace Uptime.Workflows.Core.Interfaces;

public interface IStateMachineFactory<TState, TTrigger>
{
    IStateMachine<TState, TTrigger> Create(TState initialState);
}