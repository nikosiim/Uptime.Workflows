namespace Uptime.Workflows.Core;

public interface IStateMachineFactory<TState, TTrigger>
{
    IStateMachine<TState, TTrigger> Create(TState initialState);
}