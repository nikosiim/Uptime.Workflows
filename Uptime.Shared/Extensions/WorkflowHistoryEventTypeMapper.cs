using Uptime.Shared.Enums;

namespace Uptime.Shared.Extensions;

public static class WorkflowHistoryEventTypeMapper
{
    private static readonly Dictionary<WorkflowHistoryEventType, string> EventDescriptions = new()
    {
        { WorkflowHistoryEventType.None, "" },
        { WorkflowHistoryEventType.WorkflowStarted, "Töövoog on algatatud" },
        { WorkflowHistoryEventType.WorkflowCompleted, "Töövoog on lõpetatud" },
        { WorkflowHistoryEventType.WorkflowCancelled, "Töövoog on tühistatud" },
        { WorkflowHistoryEventType.WorkflowDeleted, "Töövoog on kustutatud" },
        { WorkflowHistoryEventType.TaskCreated, "Tööülesanne on loodud" },
        { WorkflowHistoryEventType.TaskCompleted, "Tööülesanne on lõpule viidud" },
        { WorkflowHistoryEventType.TaskModified, "Tööülesanne on muudetud" },
        { WorkflowHistoryEventType.TaskRolledBack, "Tööülesanne on tagasivõetud" },
        { WorkflowHistoryEventType.TaskDeleted, "Tööülesanne on kustutatud" },
        { WorkflowHistoryEventType.WorkflowError, "Töövoo tõrge" },
        { WorkflowHistoryEventType.WorkflowComment, "Töövoo kommentaar" }
    };

    public static string GetTranslation(this WorkflowHistoryEventType eventType)
    {
        return EventDescriptions.GetValueOrDefault(eventType, "Tundmatu sündmus");
    }
}