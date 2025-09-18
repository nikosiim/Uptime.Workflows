namespace SigningWorkflow;

public static class ActivityPayloadExtensions
{
    public static string? GetTaskComment(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(PayloadInputKeys.TaskComment);
    
    private static class PayloadInputKeys
    {
        public const string TaskComment = "Task.Comment";
    }
}