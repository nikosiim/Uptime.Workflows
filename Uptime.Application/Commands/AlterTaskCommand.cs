using MediatR;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<WorkflowStatus>
{
    public int TaskId { get; init; }
    public int WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class AlterTaskCommandHandler(IWorkflowService workflowService, ITaskService taskService) 
    : IRequestHandler<AlterTaskCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        var workflow = new ApprovalWorkflow(workflowService, taskService);

        bool isRehydrated = await workflow.ReHydrateAsync(request.WorkflowId);
        if (isRehydrated)
        {
            var payload = new AlterTaskPayload(request.TaskId, request.WorkflowId, request.Storage);
            return await workflow.AlterTaskAsync(payload);
        }

        return WorkflowStatus.Invalid;
    }
}