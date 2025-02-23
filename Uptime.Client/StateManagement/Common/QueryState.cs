namespace Uptime.Client.StateManagement.Common;

public readonly record struct QueryState<T>
{
    public required T Result { get; init; }
    public required QueryStatus Status { get; init; }
}