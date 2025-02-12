using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorWorkflowContext<TData> : IWorkflowContext where TData : IReplicatorItem
{
    Dictionary<string, ReplicatorState<TData>> ReplicatorStates { get; set; }
}