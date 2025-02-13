using Uptime.Application.Common;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityProvider(IWorkflowRepository repository) 
    : ReplicatorActivityProvider(repository)
{
    public override IWorkflowActivity CreateActivity(string phaseName, object data, WorkflowTaskContext context)
    {
        if (phaseName == Phases.SigningPhase)
        {
            return new SigningTaskActivity(Repository, context)
            {
                TaskData = data.DeserializeTaskData<SigningTaskData>()
            };
        }

        return new ApprovalTaskActivity(Repository, context)
        {
            TaskData = data.DeserializeTaskData<ApprovalTaskData>()
        };
    }

    public override void OnChildInitialized(string phaseName, object data, IWorkflowActivity activity)
    {
        if (phaseName == Phases.ApprovalPhase)
        {
            var taskData = data.DeserializeTaskData<ApprovalTaskData>();
            Console.WriteLine($"Approval task initialized for {taskData.AssignedTo}");
        }
        else if (phaseName == Phases.SigningPhase)
        {
            var taskData = data.DeserializeTaskData<SigningTaskData>();
            Console.WriteLine($"Signing task initialized for {taskData.AssignedTo}");
        }
    }

    public override void OnChildCompleted(string phaseName, object data, IWorkflowActivity activity)
    {
        if (phaseName == Phases.ApprovalPhase)
        {
            var taskData = data.DeserializeTaskData<ApprovalTaskData>();
            Console.WriteLine($"Approval task completed for {taskData.AssignedTo}");
        }
        else if (phaseName == Phases.SigningPhase)
        {
            var taskData = data.DeserializeTaskData<SigningTaskData>();
            Console.WriteLine($"Signing task completed for {taskData.AssignedTo}");
        }
    }
}