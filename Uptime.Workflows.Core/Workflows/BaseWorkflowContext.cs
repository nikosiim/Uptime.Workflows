using System.Text.Json;
using System.Text.Json.Serialization;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

/// <summary>
/// The minimal, general-purpose base class for workflow context objects.
/// 
/// <para>
/// <b>What is this?</b><br/>
/// <c>BaseWorkflowContext</c> is the default implementation of <see cref="IWorkflowContext"/>.
/// It is used to persist all runtime data that a workflow instance needs to remember as it runs,
/// such as workflow outcome, custom data, and state needed for transitions.
/// </para>
///
/// <para>
/// <b>How is it used?</b><br/>
/// - Each workflow instance has its own context (or a subclass of this).
/// - All context data is persisted to the database as JSON (<see cref="Serialize"/>/<see cref="Deserialize"/>).
/// - Core properties are <see cref="Outcome"/> (the result of the workflow)
///   and <see cref="Storage"/>, a dictionary for arbitrary per-instance data (IDs, SIDs, names, custom fields).
/// - Provides utility methods for (de)serialization.
/// </para>
///
/// <para>
/// <b>When to inherit from this:</b><br/>
/// - Create your own subclass if you need to add workflow-specific fields (e.g., ApprovalWorkflowContext adds AnyTaskRejected).
/// - Use <c>BaseWorkflowContext</c> directly if you only need storage for generic workflow metadata.
/// </para>
///
/// <para>
/// <b>Typical usage pattern:</b><br/>
/// <code>
/// // Access typed data using storage helpers (see WorkflowContextExtensions)
/// var docId = context.GetDocumentId();
/// context.SetCustomField("CustomField", "value");
/// </code>
/// </para>
/// </summary>
public class BaseWorkflowContext : IWorkflowContext
{
    [JsonIgnore] 
    public WorkflowOutcome Outcome { get; set; } = WorkflowOutcome.None;
    public Dictionary<string, string?> Storage { get; set; } = new();
    public virtual string Serialize() => JsonSerializer.Serialize(this);
    public virtual void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<BaseWorkflowContext>(json);
        if (obj != null)
        {
            Outcome = obj.Outcome;
            Storage = obj.Storage;
        }
    }

    public static TContext Deserialize<TContext>(string? json) where TContext : IWorkflowContext, new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return new TContext();

        try
        {
            return JsonSerializer.Deserialize<TContext>(json) ?? new TContext();
        }
        catch (JsonException)
        {
            return new TContext();
        }
    }
}