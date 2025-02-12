using Stateless;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class StatelessStateConfigurationAdapter<TState, TTrigger>(StateMachine<TState, TTrigger>.StateConfiguration stateConfig)
    : IStateConfiguration<TState, TTrigger>
{
    public IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState state)
    {
        stateConfig.Permit(trigger, state);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> OnEntry(Action action)
    {
        stateConfig.OnEntry(action);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> OnEntryAsync(Func<Task> action)
    {
        stateConfig.OnEntryAsync(action);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> OnExit(Action action)
    {
        stateConfig.OnExit(action);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> OnExitAsync(Func<Task> action)
    {
        stateConfig.OnExitAsync(action);
        return this;
    }
}
