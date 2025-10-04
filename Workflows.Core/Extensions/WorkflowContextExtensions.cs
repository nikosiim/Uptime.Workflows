using Workflows.Core.Common;
using Workflows.Core.Interfaces;

namespace Workflows.Core.Extensions;

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
public static class WorkflowContextExtensionsCore
{
    #region AssociationName

    public static string? GetAssociationName(this IWorkflowContext context)
        => context.Storage.GetValueOrDefault(StorageKeys.AssociationName);

    #endregion

    #region DocumentId

    public static void SetDocumentId(this IWorkflowContext context, DocumentId id)
        => context.Storage[StorageKeys.DocumentId] = id.Value.ToString();

    public static DocumentId GetDocumentId(this IWorkflowContext context)
        => DocumentId.Parse(context.Storage.GetValueOrDefault(StorageKeys.DocumentId));

    #endregion

    #region Initiator

    public static void SetInitiatorSid(this IWorkflowContext context, PrincipalSid principalSid)
    {
        context.Storage[StorageKeys.InitiatorSid] = principalSid.Value;
    }

    public static PrincipalSid GetInitiatorSid(this IWorkflowContext context)
    {
        if (!context.Storage.TryGetValue(StorageKeys.InitiatorSid, out string? value) || string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Initiator SID is missing from WorkflowContext.Storage.");
        return (PrincipalSid)value;
    }

    #endregion

    #region WorkflowId

    public static WorkflowId GetWorkflowId(this IWorkflowContext context)
        => WorkflowId.Parse(context.Storage.GetValueOrDefault(StorageKeys.WorkflowId));

    public static void SetWorkflowId(this IWorkflowContext context, WorkflowId id)
        => context.Storage[StorageKeys.WorkflowId] = id.Value.ToString();

    #endregion

    #region WorkflowTemplateId

    public static WorkflowTemplateId GetWorkflowTemplateId(this IWorkflowContext context)
        => WorkflowTemplateId.Parse(context.Storage.GetValueOrDefault(StorageKeys.WorkflowTemplateId));

    public static void SetWorkflowTemplateId(this IWorkflowContext context, WorkflowTemplateId id)
        => context.Storage[StorageKeys.WorkflowTemplateId] = id.Value.ToString();

    #endregion

    #region SourceSiteUrl

    public static string GetSiteUrl(this IWorkflowContext context)
        => context.Storage.GetValueOrDefault(StorageKeys.SiteUrl)!;

    public static void SetSiteUrl(this IWorkflowContext context, string siteUrl)
        => context.Storage[StorageKeys.SiteUrl] = siteUrl;

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

    /// <summary>
    /// Workflow context storage keys.
    /// Naming: Workflow.[Workflow].[Phase].[Field]
    /// - [Workflow]: Workflow type (e.g. Approval, Signing)
    /// - [Phase]: Logical phase or role (e.g. Approval, Signing)
    /// - [Field]: Field name (DueDate, Sids, PrincipalIds, etc)
    /// Always include [Phase] if the key is phase-specific.
    /// </summary>
    private static class StorageKeys
    {
        public const string AssociationName      = "Workflow.Association.Name";
        public const string DocumentId           = "Workflow.Document.Id";
        public const string WorkflowId           = "Workflow.Id";
        public const string InitiatorSid         = "Workflow.Initiator.Sid";
        public const string WorkflowTemplateId   = "Workflow.Template.Id";
        public const string SiteUrl              = "Workflow.Site.Url";
    }
}