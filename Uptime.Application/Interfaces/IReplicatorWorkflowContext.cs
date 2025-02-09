using Uptime.Application.Common;

namespace Uptime.Application.Interfaces;

public interface IReplicatorWorkflowContext<TData> : IWorkflowContext where TData : IReplicatorItem
{
    Dictionary<string, ReplicatorState<TData>> ReplicatorStates { get; set; }
}