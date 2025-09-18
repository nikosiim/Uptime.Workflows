namespace ApprovalWorkflow;

public static class ActivityPayloadExtensions
{
    public static string? GetTaskComment(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(PayloadInputKeys.TaskComment);

    public static string? GetTaskDelegatedToSid(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(PayloadInputKeys.TaskDelegatedToSid);

    private static class PayloadInputKeys
    {
        public const string TaskComment = "Task.Comment";
        public const string TaskDelegatedToSid = "Task.DelegatedToSid";
    }
}