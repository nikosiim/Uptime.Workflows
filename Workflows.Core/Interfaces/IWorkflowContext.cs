namespace Workflows.Core.Interfaces;

public interface IWorkflowContext
{
    WorkflowOutcome Outcome { get; set; }
    Dictionary<string, string?> Storage { get; }

    string Serialize();
    void Deserialize(string json);
}