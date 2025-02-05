namespace Uptime.Web;

public static class Constants
{
    public static class DialogParams
    {
        public const string TemplateId = "TemplateId";
        public const string LibraryId = "LibraryId";
        public const string WorkflowDefinition = "WorkflowDefinition";
        public const string Document = "Document";
        public const string TaskId = "TaskId";
    }

    public static Dictionary<string, int> Libraries = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Contracts", 1 }, { "Letters", 2 }
    };

    public static class PageRoutes
    {
        public const string WorkflowSettings = "workflow-settings";
        public const string LibraryWorkflows = "library-workflow";
        public const string Workflows = "workflows";
        public const string Contracts = "contracts";
        public const string Letters = "letters";
    }
}