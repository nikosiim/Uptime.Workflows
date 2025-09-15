using SigningWorkflow;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace ApprovalWorkflow;

public class ApprovalWorkflowActivityProvider(ITaskService taskService, IHistoryService historyService, IPrincipalResolver principalResolver, IWorkflowContext workflowContext) 
    : ReplicatorActivityProvider
{
    public override IWorkflowActivity CreateActivity(IWorkflowTaskContext taskContext)
    {
        if (taskContext.PhaseId == ExtendedState.Signing.Value)
        {
            return new SigningTaskActivity(taskService, historyService, taskContext, workflowContext);
        }

        return new ApprovalTaskActivity(taskService, historyService, principalResolver, taskContext, workflowContext);
    }

    public override void OnChildInitialized(string phaseId, IWorkflowTaskContext context, IWorkflowActivity activity)
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

    public override void OnChildCompleted(string phaseId, IUserTaskActivity activity, Principal executedBy)
    {
        if (phaseId == ExtendedState.Approval.Value)
        {
            HandleApprovalPhaseChildCompleted((ApprovalTaskActivity)activity, workflowContext, executedBy);
        }
        else if (phaseId == ExtendedState.Signing.Value)
        {
            HandleSigningPhaseChildCompleted((SigningTaskActivity)activity, workflowContext, executedBy);
        }
    }

    private static void HandleApprovalPhaseChildCompleted<TContext>(ApprovalTaskActivity activity, TContext workflowContext, Principal executedBy)
    {
        if (workflowContext is ApprovalWorkflowContext approvalContext)
        {
            if (activity.TaskDelegatedToPrincipal != null)
            {
                string phaseId = ExtendedState.Approval.Value;
                Guid existingTaskGuid = activity.Context.TaskGuid;

                WorkflowTaskContext newContext = WorkflowTaskContextFactory.CreateNew(
                    phaseId: phaseId,
                    assignedTo: activity.TaskDelegatedToPrincipal.Id,
                    assignedBy: executedBy.Id,
                    description: activity.Context.GetTaskDescription(),
                    dueDate: activity.Context.DueDate);
                
                var replicatorItem = new ReplicatorItem(Guid.NewGuid(), newContext);

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