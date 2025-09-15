namespace Uptime.Workflows.Core;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(IWorkflowTaskContext taskContext);
    void OnChildInitialized(string phaseId, IWorkflowTaskContext taskContext, IWorkflowActivity activity);
    void OnChildCompleted(string phaseId, IUserTaskActivity activity);
}