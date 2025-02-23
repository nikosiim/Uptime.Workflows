using Fluxor;
using Uptime.Client.Application.Common;
using Uptime.Shared.Models.Libraries;

namespace Uptime.Client.StateManagement.Workflow;

public class WorkflowEffects(IApiService apiService)
{
    [EffectMethod(typeof(LoadDocumentsAction))]
    public async Task HandleLoadLibraryDocumentsAction(IDispatcher dispatcher)
    {
        string url = ApiRoutes.Libraries.GetDocuments.Replace("{libraryId}", "1");

        var result = await apiService.GetJsonAsync<List<LibraryDocumentResponse>>(url);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadDocumentsSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadDocumentsFailedAction(result.Error));
        }
    }
}