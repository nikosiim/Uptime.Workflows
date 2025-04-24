namespace Uptime.Workflows.Core.Common;

public sealed record StateTransition<TState, TTrigger>(
    TState Source,
    TState Destination,
    TTrigger Trigger
);