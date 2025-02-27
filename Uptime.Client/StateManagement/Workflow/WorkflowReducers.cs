using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public static class WorkflowReducers
{
    #region WorkflowDefinitions
    
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
    
    #endregion
    
    #region WorkflowTasks

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

    #endregion

    #region WorkflowHistory

    [ReducerMethod(typeof(LoadWorkflowHistoryAction))]
    public static WorkflowState ReduceLoadWorkflowHistoryAction(WorkflowState state) => state with
    {
        WorkflowHistoryQuery = new QueryState<Result<List<WorkflowHistoryData>>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowHistoryFailedAction(WorkflowState state, LoadWorkflowHistoryFailedAction action)
        => state with
        {
            WorkflowHistoryQuery = new QueryState<Result<List<WorkflowHistoryData>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowHistoryData>>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowHistorySuccessAction(WorkflowState state, LoadWorkflowHistorySuccessAction action)
    {
        List<WorkflowHistoryResponse> response = action.Response;

        List<WorkflowHistoryData> result = response.Select(entry
            => new WorkflowHistoryData
            {
                Id = entry.Id,
                WorkflowId = entry.WorkflowId,
                Description = entry.Description,
                Occurred = entry.Occurred.ToLocalTime(),
                Comment = entry.Comment,
                User = entry.User,
                Event = entry.Event
            }).ToList();

        return state with
        {
            WorkflowHistoryQuery = new QueryState<Result<List<WorkflowHistoryData>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowHistoryData>>.Success(result)
            }
        };
    }

    #endregion

    #region WorkflowDetails

    [ReducerMethod(typeof(LoadWorkflowDetailsAction))]
    public static WorkflowState ReduceLoadWorkflowDetailsAction(WorkflowState state) => state with
    {
        WorkflowDetailsQuery = new QueryState<Result<WorkflowDetails>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowDetailsFailedAction(WorkflowState state, LoadWorkflowDetailsFailedAction action)
        => state with
        {
            WorkflowDetailsQuery = new QueryState<Result<WorkflowDetails>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<WorkflowDetails>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowDetailsSuccessAction(WorkflowState state, LoadWorkflowDetailsSuccessAction action)
    {
        WorkflowDetailsResponse response = action.Response;
        
        var result = new WorkflowDetails
        {
            Id = action.WorkflowId,
            DocumentId = response.DocumentId,
            Document = response.Document,
            Originator = response.Originator,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Outcome = WorkflowResources.Get(response.Outcome),
            IsActive = response.IsActive
        };

        return state with
        {
            WorkflowDetailsQuery = new QueryState<Result<WorkflowDetails>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<WorkflowDetails>.Success(result)
            }
        };
    }

    #endregion
}