using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common;

public class ReplicatorPhaseBuilderFactory : IReplicatorPhaseBuilderFactory
{
    public IReplicatorPhaseBuilder Create(WorkflowConfiguration config)
    {
        return new ReplicatorPhaseBuilder(config.ReplicatorPhaseConfigurations);
    }
}