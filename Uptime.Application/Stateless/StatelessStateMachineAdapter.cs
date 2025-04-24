using Stateless;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Stateless;

public class StatelessStateMachineAdapter<TState, TTrigger>(TState initialState) : IStateMachine<TState, TTrigger>
{
    private readonly StateMachine<TState, TTrigger> _machine = new(initialState);

    public TState CurrentState => _machine.State;

    public void Fire(TTrigger trigger)
    {
        _machine.Fire(trigger);
    }

    public Task FireAsync(TTrigger trigger)
    {
        return _machine.FireAsync(trigger);
    }

    public IStateConfiguration<TState, TTrigger> Configure(TState state)
    {
        StateMachine<TState, TTrigger>.StateConfiguration? stateConfig = _machine.Configure(state);
        return new StatelessStateConfigurationAdapter<TState, TTrigger>(stateConfig);
    }

    public void OnTransitionCompleted(Action<StateTransition<TState, TTrigger>> callback)
    {
        _machine.OnTransitionCompleted(transition =>
        {
            var domainTransition = new StateTransition<TState, TTrigger>(
                transition.Source,
                transition.Destination,
                transition.Trigger
            );
            callback(domainTransition);
        });
    }

    public void OnTransitionCompletedAsync(Func<StateTransition<TState, TTrigger>, Task> callback)
    {
        _machine.OnTransitionCompletedAsync(async statelessTransition =>
        {
            var domainTransition = new StateTransition<TState, TTrigger>(
                statelessTransition.Source,
                statelessTransition.Destination,
                statelessTransition.Trigger
            );

            await callback(domainTransition).ConfigureAwait(false);
        });
    }
}