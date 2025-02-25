using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;

namespace Uptime.Client.StateManagement.Workflow;

public record WorkflowState
{
    public required QueryState<Result<List<LibraryDocument>>> LibraryDocumentsQuery { get; init; }
    public required QueryState<Result<List<WorkflowDefinition>>> WorkflowDefinitionsQuery { get; init; }
    public required QueryState<Result<List<WorkflowTemplate>>> WorkflowTemplatesQuery { get; init; }
}

public class WorkflowFeature : Feature<WorkflowState>
{
    public override string GetName() => "Workflow";

    protected override WorkflowState GetInitialState() => new()
    {
        LibraryDocumentsQuery = new QueryState<Result<List<LibraryDocument>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        },
        WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        },
        WorkflowTemplatesQuery = new QueryState<Result<List<WorkflowTemplate>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        }
    };
}