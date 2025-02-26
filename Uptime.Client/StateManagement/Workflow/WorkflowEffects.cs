using Fluxor;
using Uptime.Client.Application.Common;
using Uptime.Shared.Common;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Workflows;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Client.StateManagement.Workflow;

public class WorkflowEffects(IApiService apiService)
{
    [EffectMethod(typeof(LoadDocumentsAction))]
    public async Task HandleLoadLibraryDocumentsAction(IDispatcher dispatcher)
    {
        string url = ApiRoutes.Libraries.GetDocuments.Replace("{libraryId}", "1");
        Result<List<LibraryDocumentResponse>> result = await apiService.GetJsonAsync<List<LibraryDocumentResponse>>(url);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadDocumentsSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadDocumentsFailedAction(result.Error));
        }
    }

    [EffectMethod]
    public async Task HandleLoadDocumentWorkflowsAction(LoadDocumentWorkflowsAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.Documents.GetWorkflows.Replace("{documentId}", action.DocumentId.ToString());
        Result<List<DocumentWorkflowsResponse>> result = await apiService.GetJsonAsync<List<DocumentWorkflowsResponse>>(url);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadDocumentWorkflowsSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadDocumentWorkflowsFailedAction(result.Error));
        }
    }

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

    [EffectMethod]
    public async Task HandleLoadWorkflowTemplatesAction(LoadWorkflowTemplatesAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.Libraries.GetWorkflowTemplates.Replace("{libraryId}", action.LibraryId.ToString());
        Result<List<LibraryWorkflowTemplateResponse>> result = await apiService.GetJsonAsync<List<LibraryWorkflowTemplateResponse>>(url);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesSuccessAction(result.Value ?? []));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesFailedAction(result.Error));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteWorkflowTemplateAction(DeleteWorkflowTemplateAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.WorkflowTemplates.DeleteTemplate.Replace("{templateId}", action.TemplateId.ToString());
        Result<bool> result = await apiService.DeleteAsync(url);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesAction(action.LibraryId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesFailedAction(result.Error));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateWorkflowTemplateAction(UpdateWorkflowTemplateAction action, IDispatcher dispatcher)
    {
        string url = ApiRoutes.WorkflowTemplates.UpdateTemplate.Replace("{templateId}", action.TemplateId.ToString());

        var updateRequest = new
        {
            action.TemplateName,
            action.WorkflowName,
            action.WorkflowBaseId,
            action.AssociationDataJson
        };

        Result<bool> result = await apiService.UpdateAsync(url, updateRequest);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesAction(action.LibraryId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesFailedAction(result.Error));
        }
    }

    [EffectMethod]
    public async Task HandleCreateWorkflowTemplateAction(CreateWorkflowTemplateAction action, IDispatcher dispatcher)
    {
        var createRequest = new
        {
            action.TemplateName,
            action.WorkflowName,
            action.WorkflowBaseId,
            action.LibraryId,
            action.AssociationDataJson
        };

        var result = await apiService.CreateAsync<object, CreateWorkflowTemplateResponse>(ApiRoutes.WorkflowTemplates.CreateTemplate, createRequest);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesAction(action.LibraryId));
        }
        else
        {
            dispatcher.Dispatch(new LoadWorkflowTemplatesFailedAction(result.Error));
        }
    }
}