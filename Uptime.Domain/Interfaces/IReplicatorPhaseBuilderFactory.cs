using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IReplicatorPhaseBuilderFactory
{
    IReplicatorPhaseBuilder Create(WorkflowConfiguration config);
}