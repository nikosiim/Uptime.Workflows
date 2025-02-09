using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IWorkflowActivityFactory<in TData>
{
    IWorkflowActivity CreateActivity(WorkflowId workflowId, TData data, Guid taskGuid);
    void OnChildInitialized(string phaseName, TData data, IWorkflowActivity activity);
    void OnChildCompleted(string phaseName, TData data, IWorkflowActivity activity);
}