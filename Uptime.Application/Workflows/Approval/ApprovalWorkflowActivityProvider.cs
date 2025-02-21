using Uptime.Application.Common;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityProvider(IWorkflowRepository repository) : ReplicatorActivityProvider(repository)
{
    public override IWorkflowActivity CreateActivity(string phaseName, object data, WorkflowTaskContext context)
    {
        if (phaseName == ReplicatorPhases.SigningPhase)
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
        if (phaseName == ReplicatorPhases.ApprovalPhase)
        {
            var taskData = data.DeserializeTaskData<ApprovalTaskData>();
            Console.WriteLine($"Approval task initialized for {taskData.AssignedTo}");
        }
        else if (phaseName == ReplicatorPhases.SigningPhase)
        {
            var taskData = data.DeserializeTaskData<SigningTaskData>();
            Console.WriteLine($"Signing task initialized for {taskData.AssignedTo}");
        }
    }

    public override void OnChildCompleted<TContext>(string phaseName, UserTaskActivity activity, TContext workflowContext)
    {
        switch (phaseName)
        {
            case ReplicatorPhases.ApprovalPhase:
                HandleApprovalPhaseChildCompleted((ApprovalTaskActivity)activity, workflowContext);
                break;

            case ReplicatorPhases.SigningPhase:
                HandleSigningPhaseChildCompleted((SigningTaskActivity)activity, workflowContext);
                break;

            default:
                // If you expect more phases in the future, handle them here 
                // or do nothing if it's safe to ignore unknown phases.
                break;
        }
    }

    private static void HandleApprovalPhaseChildCompleted<TContext>(ApprovalTaskActivity activity, TContext workflowContext)
    {
        if (workflowContext is ApprovalWorkflowContext approvalContext)
        {
            if (activity.IsTaskDelegated)
            {
                ApprovalTaskData data = ApprovalTaskData.Copy(activity.TaskData!);
                data.AssignedTo = activity.Context.Storage.GetValueOrDefault(TaskStorageKeys.TaskDelegatedTo)!;

                approvalContext.ReplicatorStates.InsertItemAfter(ReplicatorPhases.ApprovalPhase, activity.Context.TaskGuid, new ReplicatorItem { Data = data });
            }
            else if (activity.IsTaskRejected)
            {
                approvalContext.AnyTaskRejected = true;
                approvalContext.ReplicatorStates.CancelAllItems(ReplicatorPhases.ApprovalPhase);
            }
        }
    }

    private static void HandleSigningPhaseChildCompleted<TContext>(SigningTaskActivity activity, TContext workflowContext)
    {
        
    }
}