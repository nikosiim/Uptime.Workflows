using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

public interface IWorkflowActivityFactory<in TData>
{
    IWorkflowActivity CreateActivity(TData data, WorkflowTaskContext context);
    void OnChildInitialized(string phaseName, TData data, IWorkflowActivity activity);
    void OnChildCompleted(string phaseName, TData data, IWorkflowActivity activity);
}