namespace Uptime.Workflows.Core.Interfaces;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(IWorkflowActivityContext activityContext);
    void OnChildInitialized(string phaseId, IWorkflowActivityContext activityContext, IWorkflowActivity activity);
    void OnChildCompleted(string phaseId, IUserTaskActivity activity);
}