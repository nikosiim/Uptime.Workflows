using Uptime.Application.Common;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityProvider(IWorkflowRepository repository) : ReplicatorActivityProvider(repository)
{
    public override IWorkflowActivity CreateActivity(string phaseId, object data, WorkflowTaskContext context)
    {
        if (phaseId == ExtendedState.Signing.Value)
        {
            return new SigningTaskActivity(Repository, context)
            {
                TaskData = data.DeserializeTaskData<SigningTaskData>() // TODO: kas see on õige?
            };
        }

        return new ApprovalTaskActivity(Repository, context)
        {
            TaskData = data.DeserializeTaskData<ApprovalTaskData>()
        };
    }

    public override void OnChildInitialized(string phaseId, object data, IWorkflowActivity activity)
    {
        if (phaseId == ExtendedState.Approval.Value)
        {
            // var taskData = data.DeserializeTaskData<ApprovalTaskData>();
        }
        else if (phaseId == ExtendedState.Signing.Value)
        {
            // var taskData = data.DeserializeTaskData<SigningTaskData>();
        }
    }

    public override void OnChildCompleted<TContext>(string phaseId, UserTaskActivity activity, TContext workflowContext)
    {
        if (phaseId == ExtendedState.Approval.Value)
        {
            HandleApprovalPhaseChildCompleted((ApprovalTaskActivity)activity, workflowContext);
        }
        else if (phaseId == ExtendedState.Signing.Value)
        {
            HandleSigningPhaseChildCompleted((SigningTaskActivity)activity, workflowContext);
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

                approvalContext.ReplicatorStates.InsertItemAfter(ExtendedState.Approval.Value, activity.Context.TaskGuid, new ReplicatorItem { Data = data });
            }
            else if (activity.IsTaskRejected)
            {
                approvalContext.AnyTaskRejected = true;
                approvalContext.ReplicatorStates.CancelAllItems();
            }
        }
    }

    private static void HandleSigningPhaseChildCompleted<TContext>(SigningTaskActivity activity, TContext workflowContext)
    {
        
    }
}