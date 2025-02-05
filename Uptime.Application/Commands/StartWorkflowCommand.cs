using MediatR;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Models.Common;
using Uptime.Application.Services;
using Uptime.Application.Workflows.Approval;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand(WorkflowPayload Payload) : IRequest<WorkflowStatus>;

public class StartWorkflowCommandHandler(IWorkflowService workflowService, ITaskService taskService) : IRequestHandler<StartWorkflowCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (request.Payload is ApprovalWorkflowPayload approvalPayload)
        {
            var workflow = new ApprovalWorkflow(workflowService, taskService);
            return await workflow.StartAsync(approvalPayload);
        }

        return WorkflowStatus.Invalid;
    }
}