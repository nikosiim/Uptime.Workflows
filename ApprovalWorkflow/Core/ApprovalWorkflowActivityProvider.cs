using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Extensions;
using Uptime.Workflows.Core.Interfaces;

namespace ApprovalWorkflow;

public class ApprovalWorkflowActivityProvider(IActivityActivator activator, IWorkflowContext workflowContext) 
    : ReplicatorActivityProvider
{
    public override IWorkflowActivity CreateActivity(IWorkflowActivityContext activityContext)
    {
        if (activityContext.PhaseId == ExtendedState.Signing.Value)
        {
            return activator.Create<SigningTaskActivity>(workflowContext);
        }

        return activator.Create<ApprovalTaskActivity>(workflowContext);
    }

    public override void OnChildInitialized(string phaseId, IWorkflowActivityContext context, IWorkflowActivity activity)
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

    public override void OnChildCompleted(string phaseId, IUserTaskActivity activity)
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
            if (activity.TaskDelegatedToPrincipal != null)
            {
                string phaseId = ExtendedState.Approval.Value;
                Guid existingTaskGuid = activity.TaskGuid;

                WorkflowActivityContext newContext = WorkflowActivityContextFactory.CreateNew(
                    phaseId: phaseId,
                    assignedToSid: activity.TaskDelegatedToPrincipal.Sid,
                    description: approvalContext.GetTaskApproverDescription(),
                    dueDate: approvalContext.GetTaskDueDate(TaskPhase.Approver));
                
                var replicatorItem = new ReplicatorItem(newContext);

                approvalContext.ReplicatorStates.InsertItemAfter(phaseId, existingTaskGuid, replicatorItem);
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