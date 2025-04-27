using Stateless;
using Uptime.Workflows.Core;

namespace Uptime.Application.StateMachine;

public class StatelessStateConfigurationAdapter<TState, TTrigger>(StateMachine<TState, TTrigger>.StateConfiguration stateConfig)
    : IStateConfiguration<TState, TTrigger>
{
    public IStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState state)
    {
        stateConfig.Permit(trigger, state);
        return this;
    }
    
    public IStateConfiguration<TState, TTrigger> PermitIf(TTrigger trigger, TState destinationState, Func<bool> condition)
    {
        stateConfig.PermitIf(trigger, destinationState, condition);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger, Func<TState> destinationStateSelector)
    {
        stateConfig.PermitDynamic(trigger, destinationStateSelector);
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

    public IStateConfiguration<TState, TTrigger> SubstateOf(TState superstate)
    {
        stateConfig.SubstateOf(superstate);
        return this;
    }

    public IStateConfiguration<TState, TTrigger> InitialTransition(TState initialSubstate)
    {
        stateConfig.InitialTransition(initialSubstate);
        return this;
    }
}