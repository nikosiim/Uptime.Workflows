using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IStateMachine<TState, TTrigger>
{
    TState CurrentState { get; }
    void Fire(TTrigger trigger);
    Task FireAsync(TTrigger trigger);
    IStateConfiguration<TState, TTrigger> Configure(TState state);
    void OnTransitionCompleted(Action<StateTransition<TState, TTrigger>> callback);
    void OnTransitionCompletedAsync(Func<StateTransition<TState, TTrigger>, Task> callback);
}

public interface IStateConfiguration<in TState, in TTrigger>
{
    IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState state);
    IStateConfiguration<TState, TTrigger> OnEntry(Action action);
    IStateConfiguration<TState, TTrigger> OnEntryAsync(Func<Task> action);
    IStateConfiguration<TState, TTrigger> OnExit(Action action);
    IStateConfiguration<TState, TTrigger> OnExitAsync(Func<Task> action);
    IStateConfiguration<TState, TTrigger> SubstateOf(TState superstate);
    IStateConfiguration<TState, TTrigger> InitialTransition(TState initialSubstate);
}