using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public static class WorkflowReducers
{
    #region LoadDocumentsAction

    [ReducerMethod(typeof(LoadDocumentsAction))]
    public static WorkflowState ReduceLoadDocumentsAction(WorkflowState state) => state with
    {
        LibraryDocumentsQuery = new QueryState<Result<List<LibraryDocument>>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadDocumentsFailedAction(WorkflowState state, LoadDocumentsFailedAction action)
        => state with
        {
            LibraryDocumentsQuery = new QueryState<Result<List<LibraryDocument>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<LibraryDocument>>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceDocumentsAction(WorkflowState state, LoadDocumentsSuccessAction action)
    {
        List<LibraryDocumentResponse> documents = action.Documents;

        List<LibraryDocument> result = documents.Select(document
            => new LibraryDocument
            {
                Id = document.Id, 
                Title = document.Title, 
                Description = document.Description, 
                LibraryId = 1
            }).ToList();

        return state with
        {
            LibraryDocumentsQuery = new QueryState<Result<List<LibraryDocument>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<LibraryDocument>>.Success(result)
            }
        };
    }

    #endregion

    #region LoadDocumentWorkflowsAction
    
    [ReducerMethod]
    public static WorkflowState ReduceLoadDocumentWorkflowsAction(WorkflowState state, LoadDocumentWorkflowsAction action)
    {
        return state with
        {
            DocumentWorkflowsQuery = state.DocumentWorkflowsQuery with { Status = QueryStatus.Loading }
        };
    }

    [ReducerMethod]
    public static WorkflowState ReduceLoadDocumentWorkflowsSuccessAction(WorkflowState state, LoadDocumentWorkflowsSuccessAction action)
    {
        List<DocumentWorkflow> workflows = action.Workflows
            .Select(workflow => new DocumentWorkflow
            {
                Id = workflow.Id,
                TemplateId = workflow.TemplateId,
                WorkflowTemplateName = workflow.WorkflowTemplateName,
                StartDate = workflow.StartDate,
                EndDate = workflow.EndDate,
                Outcome = WorkflowResources.Get(workflow.Outcome),
                IsActive = workflow.IsActive
            }).ToList();

        return state with
        {
            DocumentWorkflowsQuery = new QueryState<Result<List<DocumentWorkflow>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<DocumentWorkflow>>.Success(workflows)
            }
        };
    }

    [ReducerMethod]
    public static WorkflowState ReduceLoadDocumentWorkflowsFailedAction(WorkflowState state, LoadDocumentWorkflowsFailedAction action)
    {
        return state with
        {
            DocumentWorkflowsQuery = new QueryState<Result<List<DocumentWorkflow>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<DocumentWorkflow>>.Failure(action.ErrorMessage)
            }
        };
    }

    #endregion

    #region LoadWorkflowDefinitionsAction
    
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

    #endregion

    #region LoadWorkflowTemplatesAction
    
    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowTemplatesAction(WorkflowState state, LoadWorkflowTemplatesAction action) => state with
    {
        WorkflowTemplatesQuery = new QueryState<Result<List<WorkflowTemplate>>>
        {
            Result = default, Status = QueryStatus.Loading
        }
    };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowTemplatesFailedAction(WorkflowState state, LoadWorkflowTemplatesFailedAction action)
        => state with
        {
            WorkflowTemplatesQuery = new QueryState<Result<List<WorkflowTemplate>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowTemplate>>.Failure(action.ErrorMessage)
            }
        };

    [ReducerMethod]
    public static WorkflowState ReduceLoadWorkflowTemplatesSuccessAction(WorkflowState state, LoadWorkflowTemplatesSuccessAction action)
    {
        List<LibraryWorkflowTemplateResponse> templates = action.Templates;

        List<WorkflowTemplate> result = templates.Select(template
            => new WorkflowTemplate
            {
                Id = template.Id,
                WorkflowBaseId = template.WorkflowBaseId,
                Name = template.Name,
                AssociationDataJson = template.AssociationDataJson,
                Created = template.Created
            })
        .ToList();

        return state with
        {
            WorkflowTemplatesQuery = new QueryState<Result<List<WorkflowTemplate>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<WorkflowTemplate>>.Success(result)
            }
        };
    }

    [ReducerMethod]
    public static WorkflowState ReduceUpdateWorkflowTemplateAction(WorkflowState state, UpdateWorkflowTemplateAction action)
    {
        return state with
        {
            WorkflowTemplatesQuery = state.WorkflowTemplatesQuery with { Status = QueryStatus.Loading }
        };
    }

    [ReducerMethod]
    public static WorkflowState ReduceCreateWorkflowTemplateAction(WorkflowState state, CreateWorkflowTemplateAction action)
    {
        return state with
        {
            WorkflowTemplatesQuery = state.WorkflowTemplatesQuery with { Status = QueryStatus.Loading }
        };
    }
    
    [ReducerMethod(typeof(ResetWorkflowTemplatesErrorAction))]
    public static WorkflowState ReduceResetWorkflowTemplatesErrorAction(WorkflowState state) => state with
    {
        WorkflowTemplatesQuery = new QueryState<Result<List<WorkflowTemplate>>>
        {
            Result = default, Status = QueryStatus.Uninitialized
        }
    };

    #endregion
}