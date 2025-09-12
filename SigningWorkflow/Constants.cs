namespace SigningWorkflow;

public static class Constants
{
    public static class ButtonAction
    {
        public const string Signing = "Signing";
        public const string Rejection = "Rejection";
        public const string Cancellation = "Cancellation";
    }

    public static class TaskStorageKeys
    {
        public const string TaskComment = "TaskComment";
        public const string TaskDescription = "TaskDescription";
        public const string TaskDueDays = "TaskDueDays";
        public const string TaskSignerSid = "TaskSignerSid";
        public const string TaskOutcome = "TaskOutcome";
        public const string TaskResult = "TaskResult";
        public const string TaskTitle = "TaskTitle";
    }
}