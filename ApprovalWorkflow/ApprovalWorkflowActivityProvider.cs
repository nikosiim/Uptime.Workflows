using SigningWorkflow;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Services;
using static ApprovalWorkflow.Constants;

namespace ApprovalWorkflow;

public class ApprovalWorkflowActivityProvider(ITaskService taskService, IHistoryService historyService) 
    : ReplicatorActivityProvider
{
    public override IWorkflowActivity CreateActivity(WorkflowTaskContext context, object data)
    {
        if (context.PhaseId == ExtendedState.Signing.Value)
        {
            return new SigningTaskActivity(taskService, historyService, context)
            {
                TaskData = data.DeserializeTaskData<UserTaskActivityData>()
            };
        }

        return new ApprovalTaskActivity(taskService, historyService, context)
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
                string assignedTo = activity.Context.Storage.GetValueOrDefault(TaskStorageKeys.TaskDelegatedToSid)!;
                ApprovalTaskData data = ApprovalTaskData.Copy(activity.TaskData!, assignedTo);

                Guid existingTaskGuid = activity.Context.TaskGuid;
                approvalContext.ReplicatorStates.InsertItemAfter(ExtendedState.Approval.Value, existingTaskGuid, new ReplicatorItem(Guid.NewGuid(), data));
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
        if (workflowContext is ApprovalWorkflowContext approvalContext && activity.IsTaskRejected)
        {
            approvalContext.AnyTaskRejected = true;
        }
    }
}