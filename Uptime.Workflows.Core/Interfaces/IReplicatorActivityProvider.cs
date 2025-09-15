using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IReplicatorActivityProvider
{
    IWorkflowActivity CreateActivity(IWorkflowTaskContext taskContext);
    void OnChildInitialized(string phaseId, IWorkflowTaskContext taskContext, IWorkflowActivity activity);
    void OnChildCompleted(string phaseId, IUserTaskActivity activity, Principal executedBy);
}