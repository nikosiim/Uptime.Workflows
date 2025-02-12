namespace Uptime.Domain.Interfaces;

public interface IStateMachineFactory<TState, in TTrigger>
{
    IStateMachine<TState, TTrigger> Create(TState initialState);
}