using Microsoft.AspNetCore.Components;

namespace Uptime.Client.StateManagement.Common;

public abstract class StateBase : IState
{
    private readonly HashSet<EventCallback> _subscriptions = [];

    public string ServiceName => GetType().Name + "Service";

    public void ValidateCaller(string callerPath)
    {
        string callerClassName = Path.GetFileNameWithoutExtension(callerPath);
        if (!callerClassName.Contains(ServiceName))
        {
            throw new InvalidOperationException($"Service must be an instance of {ServiceName}.");
        }
    }

    public IDisposable NotifyOnChange(EventCallback callback)
    {
        _subscriptions.Add(callback);
        return new Subscription(() => _subscriptions.Remove(callback));
    }

    protected async Task NotifyChangeSubscribersAsync()
    {
        foreach (EventCallback subscription in _subscriptions)
            await subscription.InvokeAsync();
    }

    protected class Subscription(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe.Invoke();
    }
}