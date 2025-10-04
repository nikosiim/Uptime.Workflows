namespace Workflows.Core.Enums;

public enum WorkflowTrigger
{
    Start = 1,
    TaskRejected = 2,
    TaskCompleted = 3,
    AllTasksCompleted = 4,
    Cancel = 5
}