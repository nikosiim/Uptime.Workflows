namespace Uptime.Web;

public static class ApiRoutes
{
    public const string WorkflowApiClient = "WorkflowApiClient";

    public static class Libraries
    {
        public const string Base = "api/libraries";
        public const string GetLibrary = $"{Base}/{{libraryId}}";
        public const string GetDocuments = $"{Base}/{{libraryId}}/documents";
        public const string GetWorkflowTemplates = $"{Base}/{{libraryId}}/workflow-templates";
    }

    public static class Documents
    {
        public const string Base = "api/documents";
        public const string GetWorkflows = $"{Base}/{{documentId}}/workflows";
        public const string GetTasks = $"{Base}/{{documentId}}/workflow-tasks";
    }

    public static class Tasks
    {
        public const string Base = "api/workflow-tasks";
        public const string GetTask = $"{Base}/{{taskId}}";
    }

    public static class Workflows
    {
        public const string Base = "api/workflows";
        public const string GetWorkflow = $"{Base}/{{workflowId}}";
        public const string GetTasks = $"{Base}/{{workflowId}}/workflow-tasks";
        public const string GetHistories = $"{Base}/{{workflowId}}/workflow-histories";
        public const string GetHistory = $"{Base}/{{workflowId}}/workflow-histories/{{historyId}}";

        public const string StartWorkflow = $"{Base}/start-workflow";
        public const string CompleteApprovalTask = $"{Base}/complete-approval-task";
    }

    public static class WorkflowTemplates
    {
        public const string Base = "api/workflow-templates";
        public const string GetTemplate = $"{Base}/{{templateId}}";
        public const string CreateTemplate = $"{Base}";
        public const string UpdateTemplate = $"{Base}/{{templateId}}";
        public const string DeleteTemplate = $"{Base}/{{templateId}}";
    }
}