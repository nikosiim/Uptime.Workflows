using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Common
{
    public class ReplicatorPhaseConfiguration
    {
        public required Func<IWorkflowPayload, ReplicatorType> ReplicatorType { get; init; }
        public required Func<IWorkflowPayload, WorkflowId, IEnumerable<object>> ActivityData { get; init; }
    }
}