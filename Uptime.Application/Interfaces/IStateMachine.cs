namespace Uptime.Application.Interfaces;

public interface IStateMachine<TState, TTrigger>
{
    TState CurrentState { get; }
    Task FireAsync(TTrigger trigger);
    IStateConfiguration<TState, TTrigger> Configure(TState state);
}

public interface IStateConfiguration<TState, TTrigger>
{
    IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState state);
    IStateConfiguration<TState, TTrigger> OnEntry(Action action);
    IStateConfiguration<TState, TTrigger> OnEntryAsync(Func<Task> action);
    IStateConfiguration<TState, TTrigger> OnExit(Action action);
    IStateConfiguration<TState, TTrigger> OnExitAsync(Func<Task> action);
}