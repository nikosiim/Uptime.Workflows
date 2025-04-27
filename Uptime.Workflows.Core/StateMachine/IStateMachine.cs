namespace Uptime.Workflows.Core;

public interface IStateMachine<TState, TTrigger>
{
    TState State { get; }
    void Fire(TTrigger trigger);
    Task FireAsync(TTrigger trigger);
    IStateConfiguration<TState, TTrigger> Configure(TState state);
    void OnTransitionCompleted(Action<StateTransition<TState, TTrigger>> callback);
    void OnTransitionCompletedAsync(Func<StateTransition<TState, TTrigger>, Task> callback);
}

public interface IStateConfiguration<in TState, in TTrigger>
{
    IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState state);
    IStateConfiguration<TState, TTrigger> PermitIf(TTrigger trigger, TState destinationState, Func<bool> condition);
    IStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger, Func<TState> destinationStateSelector);
    IStateConfiguration<TState, TTrigger> OnEntry(Action action);
    IStateConfiguration<TState, TTrigger> OnEntryAsync(Func<Task> action);
    IStateConfiguration<TState, TTrigger> OnExit(Action action);
    IStateConfiguration<TState, TTrigger> OnExitAsync(Func<Task> action);
    IStateConfiguration<TState, TTrigger> SubstateOf(TState superstate);
    IStateConfiguration<TState, TTrigger> InitialTransition(TState initialSubstate);
}