using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Domain.Common;
using Uptime.Domain.Entities;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Queries;

public record GetModificationContextQuery(WorkflowId WorkflowId) : IRequest<ModificationContext?>;

public class GetModificationContextQueryHandler(IWorkflowDbContext dbContext, IWorkflowFactory workflowFactory, ILogger<GetModificationContextQueryHandler> logger) 
    : IRequestHandler<GetModificationContextQuery, ModificationContext?>
{
    public async Task<ModificationContext?> Handle(GetModificationContextQuery request, CancellationToken cancellationToken)
    {
        Workflow? workflowInstance = await dbContext.Workflows.AsNoTracking()
            .Include(w => w.WorkflowTemplate)
            .Where(x => x.Id == request.WorkflowId.Value)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (workflowInstance == null)
            throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} not found.");

        (IReplicatorWorkflowContext? ctx, PhaseActivity? activity, ReplicatorState? replicatorState) = 
            WorkflowReplicatorHelper.ResolveReplicatorPhaseData(workflowInstance, workflowFactory, logger);

        if (ctx is null || activity is null || replicatorState is null || !activity.UpdateEnabled || replicatorState.ReplicatorType == ReplicatorType.Parallel)
            return null;
        
        List<ReplicatorItem> activeItems = replicatorState.Items
            .Where(i => i.Status is ReplicatorItemStatus.NotStarted or ReplicatorItemStatus.InProgress)
            .ToList();

        List<ContextTask> taskItems = activeItems.OfTypeUserTaskActivity()
            .Select(i => new ContextTask
            {
                AssignedTo = i.AssignedTo, 
                TaskGuid = i.TaskGuid.ToString()
            })
            .ToList();

        return new ModificationContext
        {
            WorkflowId = workflowInstance.Id,
            PhaseId = workflowInstance.Phase,
            ContextTasks = taskItems
        };
    }
}