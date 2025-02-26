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
        Result<List<WorkflowDefinitionResponse>> result = await apiService.GetJsonAsync<List<WorkflowDefinitionResponse>>(ApiRoutes.Workflows.Base);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowDefinitionsSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowDefinitionsFailedAction(result.Error));
        }
    }
}