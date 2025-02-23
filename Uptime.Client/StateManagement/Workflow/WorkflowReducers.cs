using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Libraries;

namespace Uptime.Client.StateManagement.Workflow;

public record LoadDocumentsAction;
public record LoadDocumentsFailedAction(string ErrorMessage);
public record LoadDocumentsSuccessAction(List<LibraryDocumentResponse> Documents);

public static class WorkflowReducers
{
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

        var result = new List<LibraryDocument>();

        foreach (LibraryDocumentResponse document in documents)
        {
            result.Add(new LibraryDocument
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                LibraryId = 1
            });
        }

        return state with
        {
            LibraryDocumentsQuery = new QueryState<Result<List<LibraryDocument>>>
            {
                Status = QueryStatus.Loaded,
                Result = Result<List<LibraryDocument>>.Success(result)
            }
        };
    }
}