using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Workflows.Signing;

public class SigningWorkflow(IWorkflowService workflowService, ITaskService taskService)
    : ReplicatorWorkflowBase<SigningWorkflowContext, SigningTaskData>(workflowService, taskService)
{
    protected override List<ReplicatorState<SigningTaskData>> GetReplicatorStates()
    {
        return [WorkflowContext.SigningReplicatorState];
    }

    protected override IWorkflowActivity CreateChildActivity(SigningTaskData data)
    {
        Guid taskGuid = WorkflowContext.SigningReplicatorState.Items.First(t => t.Data == data).TaskGuid;
        var taskContext = new WorkflowTaskContext(WorkflowId, taskGuid);
        return new SigningTaskActivity(taskService, taskContext) { InitiationData = data };
    }
}