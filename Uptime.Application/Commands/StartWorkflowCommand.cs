using MediatR;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Queries;
using Uptime.Application.Workflows.Approval;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record StartWorkflowCommand(WorkflowPayload Payload) : IRequest<WorkflowStatus>;

public class StartWorkflowCommandHandler(IWorkflowService workflowService, ITaskService taskService, IMediator mediator)
    : IRequestHandler<StartWorkflowCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        WorkflowTemplateDto? workflowTemplate = await mediator.Send(new GetWorkflowTemplateQuery(request.Payload.WorkflowTemplateId), cancellationToken);
        if (workflowTemplate == null)
        {
            return WorkflowStatus.Invalid;
        }

        if (workflowTemplate.WorkflowBaseId == "16778969-6d4c-4367-9106-1b0ae4a4594f")
        {
            var workflow = new ApprovalWorkflow(workflowService, taskService);
            return await workflow.StartAsync(request.Payload);
        }
        
        
        if (workflowTemplate.WorkflowBaseId == "BA0E8F92-5030-4E24-8BC8-A2A9DF622133")
        {
            // Signing workflow
        }

        return WorkflowStatus.Invalid;
    }
}