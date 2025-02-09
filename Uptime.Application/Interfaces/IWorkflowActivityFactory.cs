using Uptime.Application.Common;
using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IWorkflowActivityFactory<in TData>
{
    IWorkflowActivity CreateActivity(WorkflowId workflowId, TData data, WorkflowTaskContext context);
    void OnChildInitialized(string phaseName, TData data, IWorkflowActivity activity);
    void OnChildCompleted(string phaseName, TData data, IWorkflowActivity activity);
}