namespace Uptime.Workflows.Core;

public static class TaskInputPayloadExtensions
{
    public static string? GetTaskComment(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(TaskInputKeys.TaskComment);

    public static string? GetTaskDelegatedToSid(this Dictionary<string, string?> inputData)
        => inputData.GetValueOrDefault(TaskInputKeys.TaskDelegatedToSid);
    
    public static class TaskInputKeys
    {
        public const string TaskComment = "Task.Comment";
        public const string TaskResult = "Task.Result";
        public const string TaskDelegatedToSid = "Task.DelegatedToSid";
    }
}