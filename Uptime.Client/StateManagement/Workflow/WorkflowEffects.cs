using Fluxor;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;

namespace Uptime.Client.StateManagement.Workflow;

public class WorkflowEffects(IApiService apiService)
{
    [EffectMethod(typeof(LoadWorkflowDefinitionsAction))]
    public async Task HandleLoadWorkflowDefinitionsAction(IDispatcher dispatcher)
    {
        Result<List<WorkflowDefinitionResponse>> result = 
            await apiService.ReadFromJsonAsync<List<WorkflowDefinitionResponse>>(ApiRoutes.Workflows.Base);

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
        Result<List<WorkflowTasksResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowTasksResponse>>(url);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTasksSuccessAction(result.Value ?? [], action.WorkflowId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTasksFailedAction(result.Error));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadWorkflowHistoryAction(LoadWorkflowHistoryAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.Workflows.GetHistories.Replace("{workflowId}", action.WorkflowId.ToString());
        Result<List<WorkflowHistoryResponse>> result = await apiService.ReadFromJsonAsync<List<WorkflowHistoryResponse>>(url);
        
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowHistorySuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowHistoryFailedAction(result.Error));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadWorkflowDetailsAction(LoadWorkflowDetailsAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.Workflows.GetWorkflow.Replace("{workflowId}", action.WorkflowId.ToString());
        Result<WorkflowDetailsResponse> result = await apiService.ReadFromJsonAsync<WorkflowDetailsResponse>(url);
        
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowDetailsSuccessAction(result.Value, action.WorkflowId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowDetailsFailedAction(result.Error));
        }
    }
}