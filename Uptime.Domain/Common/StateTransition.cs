namespace Uptime.Domain.Common;

public sealed record StateTransition<TState, TTrigger>(
    TState Source,
    TState Destination,
    TTrigger Trigger
);