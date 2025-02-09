using MediatR;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Domain.Common;
using Uptime.Shared.Enums;

namespace Uptime.Application.Commands;

public record AlterTaskCommand : IRequest<WorkflowStatus>
{
    public TaskId TaskId { get; init; }
    public WorkflowId WorkflowId { get; init; }
    public Dictionary<string, string?> Storage { get; init; } = new();
}

public class AlterTaskCommandHandler(ApprovalWorkflow approvalWorkflow) 
    : IRequestHandler<AlterTaskCommand, WorkflowStatus>
{
    public async Task<WorkflowStatus> Handle(AlterTaskCommand request, CancellationToken cancellationToken)
    {
        bool isRehydrated = await approvalWorkflow.ReHydrateAsync(request.WorkflowId);
        if (isRehydrated)
        {
            var payload = new AlterTaskPayload(request.TaskId, request.WorkflowId, request.Storage);
            return await approvalWorkflow.AlterTaskAsync(payload);
        }

        return WorkflowStatus.Invalid;
    }
}