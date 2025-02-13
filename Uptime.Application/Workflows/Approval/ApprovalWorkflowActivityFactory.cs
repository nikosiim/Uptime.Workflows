using Uptime.Application.Common;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityFactory(IWorkflowRepository repository) 
    : BaseWorkflowActivityFactory<ApprovalTaskData>(repository)
{
    public override IWorkflowActivity CreateActivity(string phaseName, ApprovalTaskData data, WorkflowTaskContext context)
    {
        if (phaseName == Phases.SigningPhase)
        {
            return new SigningTaskActivity(Repository, context)
            {
                TaskData = data
            };
        }

        return new ApprovalTaskActivity(Repository, context)
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
