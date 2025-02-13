using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.Common
{
    public class ReplicatorPhaseConfiguration
    {
        public required Func<IWorkflowPayload, ReplicatorType> ReplicatorType { get; init; }
        public required Func<IWorkflowPayload, WorkflowId, IEnumerable<object>> ActivityData { get; init; }
    }
}