namespace Uptime.Workflows.Core;

public sealed record StateTransition<TState, TTrigger>(
    TState Source,
    TState Destination,
    TTrigger Trigger
);