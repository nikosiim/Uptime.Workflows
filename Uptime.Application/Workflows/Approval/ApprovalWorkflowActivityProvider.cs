using Uptime.Application.Common;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using static Uptime.Application.Workflows.Approval.ApprovalWorkflow;
using static Uptime.Shared.GlobalConstants;

namespace Uptime.Application.Workflows.Approval;

public class ApprovalWorkflowActivityProvider(IWorkflowRepository repository) 
    : ReplicatorActivityProvider(repository)
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
        if (phaseName == ReplicatorPhases.ApprovalPhase)
        {
            var taskActivity = (ApprovalTaskActivity)activity;
            if (taskActivity.IsTaskDelegated)
            {
                string? delegatedTo = taskActivity.Context.Storage.GetValueOrDefault(TaskStorageKeys.TaskDelegatedTo);
                if (!string.IsNullOrWhiteSpace(delegatedTo))
                {
                    ApprovalTaskData data = ApprovalTaskData.Copy(taskActivity.TaskData!);
                    data.AssignedTo = delegatedTo;

                    if (workflowContext is ApprovalWorkflowContext approvalContext)
                    {
                        ReplicatorState replicatorState = approvalContext.ReplicatorStates[phaseName];
                        replicatorState.Items.Add(new ReplicatorItem()
                        {
                            Data = data
                        });
                    }
                }
            }
        }
        else if (phaseName == ReplicatorPhases.SigningPhase)
        {
            
        }
    }
}