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

    public static class TaskStorageKeys
    {
        public const string TaskComment = "TaskComment";
        public const string TaskDelegatedTo = "TaskDelegatedTo";
        public const string TaskDescription = "TaskDescription";
        public const string SignerTask = "SignerTask";
        public const string TaskDueDate = "TaskDueDate";
        public const string TaskDueDays = "TaskDueDays";
        public const string TaskEditor = "TaskEditor";
        public const string TaskExecutors = "TaskExecutors";
        public const string TaskSigners = "TaskSigners";
        public const string TaskOutcome = "TaskOutcome";
        public const string TaskResult = "TaskResult";
        public const string TaskTitle = "TaskTitle";
    }

    public static class WorkflowStorageKeys
    {
        public const string AssociationName = "AssociationName";
        public const string ReplicatorType = "ReplicatorType";
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