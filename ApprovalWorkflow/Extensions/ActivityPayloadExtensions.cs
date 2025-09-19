namespace ApprovalWorkflow;

public static class ActivityPayloadExtensions
{
    public static string? GetTaskComment(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(StorageKeys.TaskComment);

    public static string? GetTaskDelegatedToSid(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(StorageKeys.TaskDelegatedToSid);

    /// <summary>
    /// Activity payload storage keys.
    /// Naming: Payload.[Workflow].[Phase].[Field]
    /// - [Workflow]: Workflow type
    /// - [Phase]: Phase or activity name (if needed)
    /// - [Field]: Specific input or output field
    /// Only include phase if payload varies per phase/activity.
    /// </summary>
    private static class StorageKeys
    {
        public const string TaskComment        = "Payload.Approval.Task.Comment";
        public const string TaskDelegatedToSid = "Payload.Approval.Task.DelegatedTo.Sid";
    }
}