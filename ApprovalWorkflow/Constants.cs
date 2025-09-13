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

    // TODO: redesign so all to be similar
    public static class TaskStorageKeys
    {
        public const string AssociationName = "AssociationName";
        public const string TaskComment = "TaskComment";
        public const string TaskDelegatedToSid = "TaskDelegatedToSid";
        public const string TaskOutcome = "TaskOutcome";
        public const string TaskResult = "TaskResult";
        public const string TaskTitle = "TaskTitle";
    }
}