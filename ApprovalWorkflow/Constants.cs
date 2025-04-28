namespace ApprovalWorkflow;

public static class Constants
{
    public static class ButtonAction
    {
        public const string Signing = "Signing";
        public const string Approval = "Approval";
        public const string Rejection = "Rejection";
        public const string Delegation = "Delegation";
        public const string Cancellation = "Cancellation";
    }

    public static class TaskStorageKeys
    {
        public const string TaskComment = "TaskComment";
        public const string TaskDelegatedTo = "TaskDelegatedTo";
        public const string TaskDescription = "TaskDescription";
        public const string SignerTask = "SignerTask";
        public const string TaskDueDate = "TaskDueDate";
        public const string TaskEditor = "TaskEditor";
        public const string TaskExecutors = "TaskExecutors";
        public const string TaskSigners = "TaskSigners";
        public const string TaskOutcome = "TaskOutcome";
        public const string TaskResult = "TaskResult";
        public const string TaskTitle = "TaskTitle";
    }
}