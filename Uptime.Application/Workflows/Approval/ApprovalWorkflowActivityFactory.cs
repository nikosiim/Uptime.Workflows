using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityFactory(ITaskService taskService) : IWorkflowActivityFactory<ApprovalTaskData>
{
    public IWorkflowActivity CreateActivity(ApprovalTaskData data, WorkflowTaskContext context)
    {
        return new ApprovalTaskActivity(taskService, context)
        {
            InitiationData = data
        };
    }

    public void OnChildInitialized(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (phaseName == Phases.ApprovalPhase)
        {
            Console.WriteLine($"Approval task initialized for {data.AssignedTo}");
        }
        else if (phaseName == Phases.SigningPhase)
        {
            Console.WriteLine($"Signing task initialized for {data.AssignedTo}");
        }
    }

    public void OnChildCompleted(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
    {
        if (phaseName == Phases.ApprovalPhase)
        {
            Console.WriteLine($"Approval task completed for {data.AssignedTo}");
        }
        else if (phaseName == Phases.SigningPhase)
        {
            Console.WriteLine($"Signing task completed for {data.AssignedTo}");
        }
    }
}
