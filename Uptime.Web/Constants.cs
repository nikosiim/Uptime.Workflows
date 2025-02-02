namespace Uptime.Web;

public static class Constants
{
    public static class DialogParameters
    {
        public const string TemplateId = "TemplateId";
        public const string LibraryId = "LibraryId";
        public const string WorkflowDefinition = "WorkflowDefinition";
    }

    public static Dictionary<string, int> Libraries = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Contracts", 1 }, { "Letters", 2 }
    };
}