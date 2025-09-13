using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core;

/// <summary>
/// Provides strongly-typed extension methods for working with the <see cref="IWorkflowContext.Storage"/> dictionary.
/// 
/// <para>
/// This static class centralizes all parsing, reading, and writing of workflow context data fields
/// (such as <c>WorkflowId</c>, <c>DocumentId</c>, <c>PrincipalId</c>, etc.), hiding string key access and
/// serialization logic from the rest of the codebase.
/// </para>
///
/// <para>
/// <b>Usage:</b> Call these methods on any <see cref="IWorkflowContext"/> instance to get/set typed values,
/// rather than manipulating the underlying <c>Dictionary&lt;string, string?&gt;</c> directly.
/// This keeps business and workflow logic clean, safe, and easily maintainable.
/// </para>
/// 
/// <para>
/// <b>Example:</b>
/// <code>
/// var workflowId = context.GetWorkflowId();
/// context.SetWorkflowId(new WorkflowId(123));
/// </code>
/// </para>
/// </summary>
public static class WorkflowContextExtensions
{
    #region AssociationName
    public static string? GetAssociationName(this IWorkflowContext context)
        => context.Storage.GetValueOrDefault(WorkflowStorageKeys.AssociationName);

    public static void SetAssociationName(this IWorkflowContext context, string? value)
    {
        if (value == null)
            context.Storage.Remove(WorkflowStorageKeys.AssociationName);
        else
            context.Storage[WorkflowStorageKeys.AssociationName] = value;
    }

    #endregion

    #region DocumentId
    public static void SetDocumentId(this IWorkflowContext context, DocumentId id)
        => context.Storage[WorkflowStorageKeys.DocumentId] = id.Value.ToString();

    public static DocumentId GetDocumentId(this IWorkflowContext context)
        => DocumentId.Parse(context.Storage.GetValueOrDefault(WorkflowStorageKeys.DocumentId));

    #endregion

    #region Initiator

    public static void SetInitiator(this IWorkflowContext context, Principal principal, string principalSid)
    {
        context.Storage[WorkflowStorageKeys.Initiator.Sid] = principalSid;
        context.Storage[WorkflowStorageKeys.Initiator.Name] = principal.Name;
        context.Storage[WorkflowStorageKeys.Initiator.PrincipalId] = principal.Id.ToString();

        if (!string.IsNullOrEmpty(principal.Email))
            context.Storage[WorkflowStorageKeys.Initiator.Email] = principal.Email;
    }

    public static PrincipalId GetInitiatorId(this IWorkflowContext context)
    {
        if (!context.Storage.TryGetValue(WorkflowStorageKeys.Initiator.PrincipalId, out string? value) || string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Initiator PrincipalId is missing from WorkflowContext.Storage.");
        return PrincipalId.Parse(value);
    }

    public static string? GetInitiatorName(this IWorkflowContext context)
    {
        return context.Storage.GetValueOrDefault(WorkflowStorageKeys.Initiator.Name);
    }

    #endregion
    
    #region WorkflowId

    public static WorkflowId GetWorkflowId(this IWorkflowContext context)
        => WorkflowId.Parse(context.Storage.GetValueOrDefault(WorkflowStorageKeys.WorkflowId));

    public static void SetWorkflowId(this IWorkflowContext context, WorkflowId id)
        => context.Storage[WorkflowStorageKeys.WorkflowId] = id.Value.ToString();

    #endregion

    #region WorkflowTemplateId

    public static WorkflowTemplateId GetWorkflowTemplateId(this IWorkflowContext context)
        => WorkflowTemplateId.Parse(context.Storage.GetValueOrDefault(WorkflowStorageKeys.WorkflowTemplateId));

    public static void SetWorkflowTemplateId(this IWorkflowContext context, WorkflowTemplateId id)
        => context.Storage[WorkflowStorageKeys.WorkflowTemplateId] = id.Value.ToString();

    #endregion

    #region Custom

    public static void SetCustomField(this IWorkflowContext context, string key, string? value)
    => context.Storage[key] = value;

    public static string? GetCustomField(this IWorkflowContext context, string key)
        => context.Storage.GetValueOrDefault(key);

    #endregion

    #region Storage Helpers

    /// <summary>
    /// Merges all keys from <paramref name="updated"/> into <paramref name="existing"/>, overwriting existing keys.
    /// </summary>
    public static void MergeWith(this Dictionary<string, string?> existing, Dictionary<string, string?> updated)
    {
        foreach (KeyValuePair<string, string?> kvp in updated)
            existing[kvp.Key] = kvp.Value;
    }

    #endregion

    private static class WorkflowStorageKeys
    {
        public const string WorkflowId = "WorkflowId";
        public const string DocumentId = "DocumentId";
        public const string WorkflowTemplateId = "WorkflowTemplateId";
        public const string AssociationName = "AssociationName";

        public static class Initiator
        {
            public const string PrincipalId = "initiator:principalId";
            public const string Name = "initiator:name";
            public const string Sid = "initiator:sid";
            public const string Email = "initiator:email";
        }
    }
}