using MediatR;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record AlterTaskCommand(AlterTaskPayload Payload) : IRequest<WorkflowStatus>;

public class AlterTaskCommandHandler(IWorkflowService workflowService, ITaskService taskService) 
    : IRequestHandler<AlterTaskCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        var workflow = new ApprovalWorkflow(workflowService, taskService);

        bool isRehydrated = await workflow.ReHydrateAsync(request.Payload.TaskId);
        if (isRehydrated)
        {
            return await workflow.AlterTaskAsync(request.Payload);
        }

        return WorkflowStatus.Invalid;
    }
}