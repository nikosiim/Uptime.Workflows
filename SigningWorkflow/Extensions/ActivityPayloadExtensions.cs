namespace SigningWorkflow;

public static class ActivityPayloadExtensions
{
    public static string? GetTaskComment(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(StorageKeys.TaskComment);
    
    private static class StorageKeys
    {
        public const string TaskComment = "Payload.Signing.Task.Comment";
    }
}