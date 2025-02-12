using Microsoft.Extensions.Logging;
using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Common;

public class WorkflowFactory : IWorkflowFactory
{
    private readonly ILogger<WorkflowFactory> _logger;
    private readonly Dictionary<Guid, IWorkflowMachine> _workflowMap;

    public WorkflowFactory(IEnumerable<IWorkflowMachine> workflows, ILogger<WorkflowFactory> logger)
    {
        _logger = logger;

        // Build the mapping based on a unique identifier for each workflow.
        _workflowMap = workflows.ToDictionary(GetWorkflowBaseId, workflow => workflow);
    }

    public async Task<WorkflowPhase> StartWorkflowAsync(Guid workflowBaseId, IWorkflowPayload payload, CancellationToken cancellationToken)
    {
        if (_workflowMap.TryGetValue(workflowBaseId, out IWorkflowMachine? workflow))
        {
            return await workflow.StartAsync(payload, cancellationToken);
        }

        _logger.LogWarning("No workflow found for WorkflowBaseId: {WorkflowBaseId}", workflowBaseId);

        return WorkflowPhase.Invalid;
    }

    public IWorkflowMachine? GetWorkflow(Guid workflowBaseId)
    {
        return _workflowMap.GetValueOrDefault(workflowBaseId);
    }

    private static Guid GetWorkflowBaseId(IWorkflowMachine workflow)
    {
        // Determine the base ID based on the workflow's type.
        return workflow switch
        {
            ApprovalWorkflow => Guid.Parse("16778969-6d4c-4367-9106-1b0ae4a4594f"),
            SigningWorkflow => Guid.Parse("BA0E8F92-5030-4E24-8BC8-A2A9DF622133"),
            _ => throw new InvalidOperationException("Unknown workflow type registered")
        };
    }
}