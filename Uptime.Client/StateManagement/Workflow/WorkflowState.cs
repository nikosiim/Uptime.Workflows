using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;

namespace Uptime.Client.StateManagement.Workflow;

public record WorkflowState
{
    public required QueryState<Result<List<WorkflowDefinition>>> WorkflowDefinitionsQuery { get; init; }
}

public class WorkflowFeature : Feature<WorkflowState>
{
    public override string GetName() => "Workflow";

    protected override WorkflowState GetInitialState() => new()
    {
        WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        }
    };
}