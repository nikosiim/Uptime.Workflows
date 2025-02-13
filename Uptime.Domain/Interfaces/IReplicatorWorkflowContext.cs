using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorWorkflowContext : IWorkflowContext
{
    Dictionary<string, ReplicatorState> ReplicatorStates { get; set; }
}