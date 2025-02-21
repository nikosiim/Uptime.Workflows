namespace Uptime.Web;

public static class Constants
{
    public static class DialogParams
    {
        public const string Document = "Document";
        public const string EventType = "EventType";
        public const string LibraryId = "LibraryId";
        public const string TaskId = "TaskId";
        public const string TemplateId = "TemplateId";
        public const string WorkflowDefinition = "WorkflowDefinition";
        public const string WorkflowId = "WorkflowId";
    }

    public static Dictionary<string, int> Libraries = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Contracts", 1 }, { "Letters", 2 }
    };

    public static class PageRoutes
    {
        public const string WorkflowSettings = "workflow-settings";
        public const string LibraryWorkflows = "library-workflow";
        public const string Workflow = "workflow";
        public const string WorkflowTask = "workflow-task";
        public const string Contracts = "contracts";
        public const string Letters = "letters";
    }

    public static readonly string[] AvailableUsers =
    [
        "Klient Üks", "Klient Kaks", "Klient Kolm", "Klient Neli", "Klient Viis",
        "Lauri Saar", "Marika Oja", "Jana Pärn", "Piia Saar", "Urve Oja", "Riin Koppel", "Peeter Sepp", "Kristina Kroon",
        "Markus Lepik", "Marta Laine", "Anton Rebane", "Signe Kask", "Viljar Laine"
    ];
}