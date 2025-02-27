using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
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
        List<WorkflowDefinitionResponse> definitions = action.Response;

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


    [ReducerMethod(typeof(LoadWorkflowTasksAction))]
    public static WorkflowState ReduceLoadWorkflowTasksAction(WorkflowState state) => state with
    {
        WorkflowTasksQuery = new QueryState<Result<List<WorkflowTaskData>>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowTasksFailedAction(WorkflowState state, LoadWorkflowTasksFailedAction action)
        => state with
        {
            WorkflowTasksQuery = new QueryState<Result<List<WorkflowTaskData>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowTaskData>>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowTasksSuccessAction(WorkflowState state, LoadWorkflowTasksSuccessAction action)
    {
        List<WorkflowTasksResponse> response = action.Response;

        List<WorkflowTaskData> result = response.Select(task => new WorkflowTaskData
        {
            Id = task.Id,
            AssignedTo = task.AssignedTo,
            Description = task.Description,
            DueDate = task.DueDate,
            EndDate = task.EndDate,
            Status = WorkflowTaskResources.Get(task.Status),
            InternalStatus = task.InternalStatus,
            StorageJson = task.StorageJson,
            WorkflowId = action.WorkflowId
        }).ToList();

        return state with
        {
            WorkflowTasksQuery = new QueryState<Result<List<WorkflowTaskData>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowTaskData>>.Success(result)
            }
        };
    }
}