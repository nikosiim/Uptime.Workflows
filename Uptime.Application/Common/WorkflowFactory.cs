using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Enums;

namespace Uptime.Application.Common;

public class WorkflowFactory : IWorkflowFactory
{
    private readonly Dictionary<Guid, IWorkflowMachine> _workflowMap;

    public WorkflowFactory(IEnumerable<IWorkflowMachine> workflows)
    {
        _workflowMap = workflows.ToDictionary(GetWorkflowBaseId, workflow => workflow);
    }

    public async Task<WorkflowPhase> StartWorkflowAsync(Guid workflowBaseId, IWorkflowPayload payload)
    {
        if (_workflowMap.TryGetValue(workflowBaseId, out IWorkflowMachine? workflow))
        {
            return await workflow.StartAsync(payload);
        }

        Console.WriteLine($"No workflow found for WorkflowBaseId: {workflowBaseId}");
        return WorkflowPhase.Invalid;
    }

    private static Guid GetWorkflowBaseId(IWorkflowMachine workflow)
    {
        return workflow switch
        {
            ApprovalWorkflow => Guid.Parse("16778969-6d4c-4367-9106-1b0ae4a4594f"),
            SigningWorkflow => Guid.Parse("BA0E8F92-5030-4E24-8BC8-A2A9DF622133"),
            _ => throw new InvalidOperationException("Unknown workflow type registered")
        };
    }
}