using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public static class WorkflowReducers
{
    [ReducerMethod(typeof(LoadWorkflowDefinitionsAction))]
    public static WorkflowState ReduceLoadWorkflowDefinitionsAction(WorkflowState state) => state with
    {
        WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowDefinitionsFailedAction(WorkflowState state, LoadWorkflowDefinitionsFailedAction action)
        => state with
        {
            WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowDefinition>>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowDefinitionsSuccessAction(WorkflowState state, LoadWorkflowDefinitionsSuccessAction action)
    {
        List<WorkflowDefinitionResponse> definitions = action.Definitions;

        List<WorkflowDefinition> result = definitions.Select(
            wd => new WorkflowDefinition
            {
                Id = wd.Id,
                Name = wd.Name,
                DisplayName = wd.DisplayName,
                Actions = wd.Actions?.ToList(),
                ReplicatorActivities = wd.Phases?.Select(
                    pa => new PhaseActivity
                    {
                        PhaseId = pa.PhaseId, SupportsSequential = pa.SupportsSequential,
                        SupportsParallel = pa.SupportsParallel, Actions = pa.Actions?.ToList()
                    }).ToList(),
                FormsConfiguration = Constants.WorkflowMappings.FirstOrDefault(x => x.Id == wd.Id)
            })
            .ToList();

        return state with
        {
            WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowDefinition>>.Success(result)
            }
        };
    }
}