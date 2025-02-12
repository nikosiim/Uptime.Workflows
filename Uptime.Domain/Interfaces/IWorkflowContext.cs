namespace Uptime.Domain.Interfaces;

public interface IWorkflowContext
{
    Dictionary<string, string?> Storage { get; protected set; }

    string Serialize();
    void Deserialize(string json);
}