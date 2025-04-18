namespace Uptime.Application;

internal static class Constants
{
    public static class ButtonAction
    {
        public const string Signing = "Signing";
        public const string Approval = "Approval";
        public const string Rejection = "Rejection";
        public const string Delegation = "Delegation";
        public const string Cancellation = "Cancellation";
        public const string Termination = "Termination";
    }

    public static class TaskStorageKeys
    {
        public const string TaskComment = "TaskComment";
        public const string TaskDelegatedTo = "TaskDelegatedTo";
        public const string TaskDescription = "TaskDescription";
        public const string SignerTask = "SignerTask";
        public const string TaskDueDate = "TaskDueDate";
        public const string TaskDueDays = "TaskDueDays";
        public const string TaskEditor = "TaskEditor";
        public const string TaskExecutors = "TaskExecutors";
        public const string TaskSigners = "TaskSigners";
        public const string TaskOutcome = "TaskOutcome";
        public const string TaskResult = "TaskResult";
        public const string TaskTitle = "TaskTitle";
    }

    public static class WorkflowStorageKeys
    {
        public const string AssociationName = "AssociationName";
        public const string ReplicatorType = "ReplicatorType";
    }
}