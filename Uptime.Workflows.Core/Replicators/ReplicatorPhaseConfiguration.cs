using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core
{
    public class ReplicatorPhaseConfiguration
    {
        public required Func<IWorkflowPayload, ReplicatorType> ReplicatorType { get; init; }
        public required Func<IWorkflowPayload, WorkflowId, IEnumerable<object>> ActivityData { get; init; }
    }
}