using Microsoft.AspNetCore.Components;

namespace Uptime.Client.StateManagement.Common;

public interface IState
{
    string ServiceName { get; }
    void ValidateCaller(string callerPath);
    IDisposable NotifyOnChange(EventCallback callback);
}