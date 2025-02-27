using Fluxor;
using Uptime.Client.Application.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public class WorkflowEffects(IApiService apiService)
{
    [EffectMethod(typeof(LoadWorkflowDefinitionsAction))]
    public async Task HandleLoadWorkflowDefinitionsAction(IDispatcher dispatcher)
    {
        Result<List<WorkflowDefinitionResponse>> result = 
            await apiService.ReadFromJsonAsync<List<WorkflowDefinitionResponse>>(ApiRoutes.Workflows.Base, CancellationToken.None);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowDefinitionsSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowDefinitionsFailedAction(result.Error));
        }
    }

    [EffectMethod]
    public async Task HandleLoadLibraryDocumentsAction(LoadWorkflowTasksAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.Workflows.GetTasks.Replace("{workflowId}", action.WorkflowId.ToString());
        Result<List<WorkflowTasksResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowTasksResponse>>(url, CancellationToken.None);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTasksSuccessAction(result.Value ?? [], action.WorkflowId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTasksFailedAction(result.Error));
        }
    }
}