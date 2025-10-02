namespace Uptime.Client;

public static class ApiRoutes
{
    public const string WorkflowApiClient = "WorkflowApiClient";
    
    public static class Documents
    {
        private const string Base = "api/documents";
        public const string GetWorkflows = $"{Base}/{{documentId}}/workflows";
        public const string GetTasks = $"{Base}/{{documentId}}/workflow-tasks";
    }

    public static class Tasks
    {
        private const string Base = "api/workflow-tasks";
        public const string GetTask = $"{Base}/{{taskGuid}}";
        public const string AlterTask = $"{Base}/{{taskGuid}}/update";
    }

    public static class Workflows
    {
        public const string Base = "api/workflows";
        public const string GetWorkflow = $"{Base}/{{workflowId}}";
        public const string GetTasks = $"{Base}/{{workflowId}}/workflow-tasks";
        public const string GetHistories = $"{Base}/{{workflowId}}/workflow-histories";
        public const string GetModificationContext = $"{Base}/{{workflowId}}/workflow-context";
        public const string GetHistory = $"{Base}/{{workflowId}}/workflow-histories/{{historyId}}";

        public const string StartWorkflow = $"{Base}/start-workflow";
        public const string ModifyWorkflow = $"{Base}/{{workflowId}}/modify-workflow";
        public const string CancelWorkflow = $"{Base}/{{workflowId}}/cancel-workflow";
        public const string DeleteWorkflow = $"{Base}/{{workflowId}}/delete-workflow";
    }

    public static class WorkflowTemplates
    {
        public const string Base = "api/workflow-templates";
        public const string GetTemplate = $"{Base}/{{templateId}}";
        public const string CreateTemplate = $"{Base}";
        public const string UpdateTemplate = $"{Base}/{{templateId}}";
        public const string DeleteTemplate = $"{Base}/{{templateId}}";
        public const string GetByLibrary = $"{Base}/by-library/{{libraryId}}";
    }
}