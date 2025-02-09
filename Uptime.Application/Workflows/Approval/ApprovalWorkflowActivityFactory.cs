using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityFactory(ITaskService taskService) : IWorkflowActivityFactory<ApprovalTaskData>
{
    public IWorkflowActivity CreateActivity(WorkflowId workflowId, ApprovalTaskData data)
    {
        return new ApprovalTaskActivity(taskService, new WorkflowTaskContext(workflowId))
        {
            InitiationData = data
        };
    }

    public void OnChildInitialized(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (phaseName == "ApprovalInProgress")
        {
            Console.WriteLine($"Approval task initialized for {data.AssignedTo}");
        }
        else if (phaseName == "SigningPhase")
        {
            Console.WriteLine($"Signing task initialized for {data.AssignedTo}");
        }
    }

    public void OnChildCompleted(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (phaseName == "ApprovalInProgress")
        {
            Console.WriteLine($"Approval task completed for {data.AssignedTo}");
        }
        else if (phaseName == "SigningPhase")
        {
            Console.WriteLine($"Signing task completed for {data.AssignedTo}");
        }
    }
}
