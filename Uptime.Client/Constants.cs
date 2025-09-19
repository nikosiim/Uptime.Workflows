using System.Collections.Immutable;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Presentation.Dialogs;

namespace Uptime.Client;

public static class Constants
{
    public static class ButtonAction
    {
        public const string Signing = "Signing";
        public const string Approval = "Approval";
        public const string Rejection = "Rejection";
        public const string Delegation = "Delegation";
        public const string Cancellation = "Cancellation";
        public const string Termination = "Termination";
    }

    public static class ConfigurationKeys
    {
        public const string ClientPrefix = "Client:";
        public const string AADSection = $"{ClientPrefix}AAD";
        public const string DefaultScope = $"{AADSection}:DefaultScope";
        public const string ApiPrefix = "ApiSettings:";
        public const string WorkflowApiUrl = $"{ApiPrefix}WorkflowApiUrl";
    }

    public static class DialogParams
    {
        public const string Document = "Document";
        public const string LibraryId = "LibraryId";
        public const string TemplateId = "TemplateId";
        public const string WorkflowDefinition = "WorkflowDefinition";
        public const string WorkflowDetails = "WorkflowDetails";
        public const string WorkflowId = "WorkflowId";
        public const string ModificationContext = "ModificationContext";
    }

    public static readonly ImmutableDictionary<string, int> Libraries =
        ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, [
            new KeyValuePair<string, int>("Contracts", 1),
            new KeyValuePair<string, int>("Letters", 2)
        ]);

    public static class PageRoutes
    {
        public const string WorkflowSettings = "workflow-settings";
        public const string LibraryWorkflows = "library-workflow";
        public const string Workflow = "workflow";
        public const string WorkflowTask = "workflow-task";
        public const string Contracts = "contracts";
        public const string Letters = "letters";
    }
    
    public static class StorageKeys
    {
        public static class Workflow
        {
            public static class Core
            {
                public const string AssociationName      = "Workflow.Association.Name";
                public const string DocumentId           = "Workflow.Document.Id";
                public const string WorkflowTemplateId   = "Workflow.Template.Id";
            }

            public static class Approval
            {
                public const string ReplicatorType           = "Workflow.Approval.Task.Approver.ReplicatorType";
                public const string TaskApprovalDueDate      = "Workflow.Approval.Task.Approver.DueDate";
                public const string TaskApproverDescription  = "Workflow.Approval.Task.Approver.Description";
                public const string TaskApproverSids         = "Workflow.Approval.Task.Approver.Sids";
                public const string TaskSignerDueDate        = "Workflow.Approval.Task.Signer.DueDate";
                public const string TaskSignerDescription    = "Workflow.Approval.Task.Signer.Description";
                public const string TaskSignerSids           = "Workflow.Approval.Task.Signer.Sids";
            }

            public static class Signing
            {
                public const string TaskDueDays      = "Workflow.Signing.Task.DueDays";
                public const string TaskDescription  = "Workflow.Signing.Task.Description";
                public const string TaskSids         = "Workflow.Signing.Task.Sids";
            }
        }

        public static class Activity
        {
            public static class Approval
            {
                public const string TaskTitle   = "Activity.Approval.Task.Title";
                public const string TaskComment = "Activity.Approval.Task.Comment";
            }

            public static class Signing
            {
                public const string TaskTitle   = "Activity.Signing.Task.Title";
                public const string TaskComment = "Activity.Signing.Task.Comment";
            }
        }

        public static class Payload
        {
            public static class Approval
            {
                public const string TaskComment        = "Payload.Approval.Task.Comment";
                public const string TaskDelegatedToSid = "Payload.Approval.Task.DelegatedTo.Sid";
            }

            public static class Signing
            {
                public const string TaskComment = "Payload.Signing.Task.Comment";
            }
        }
    }

    public static IReadOnlyList<FormsConfiguration> WorkflowMappings { get; } = new List<FormsConfiguration>
        {
            new()
            {
                Id = "16778969-6d4c-4367-9106-1b0ae4a4594f",
                InitiationPage = "approval-initiation",
                AssociationDialogType = typeof(ApprovalAssociationDialog),
                ModificationDialogType = typeof(ApprovalModificationDialog)
            },
            new()
            {
                Id = "52C90EB4-7F3D-4A1D-B469-3F3B064F76D7",
                InitiationPage = "signing-initiation",
                AssociationDialogType = typeof(SigningAssociationDialog),
                ModificationDialogType = null
            }
        };
}