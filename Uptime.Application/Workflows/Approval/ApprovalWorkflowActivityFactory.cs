using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityFactory(IWorkflowTaskRepository taskService) : BaseWorkflowActivityFactory<ApprovalTaskData>(taskService)
{
    public override IWorkflowActivity CreateActivity(ApprovalTaskData data, WorkflowTaskContext context)
    {
        return new ApprovalTaskActivity(TaskService, context)
        {
            TaskData = data
        };
    }

    public override void OnChildInitialized(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
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

    public override void OnChildCompleted(string phaseName, ApprovalTaskData data, IWorkflowActivity activity)
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
