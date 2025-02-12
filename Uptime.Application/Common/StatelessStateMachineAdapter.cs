﻿using Stateless;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

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
}