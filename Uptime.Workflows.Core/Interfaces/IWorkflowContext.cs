using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowContext
{
    WorkflowOutcome Outcome { get; set; }
    Dictionary<string, string?> Storage { get; protected set; }

    string Serialize();
    void Deserialize(string json);
}