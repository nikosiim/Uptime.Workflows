using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;
using Fluxor;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Workflow;
using Uptime.Shared.Common;

namespace Uptime.Client.Presentation.Forms;

public abstract class WorkflowAssocForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Inject] public IDispatcher Dispatcher { get; set; } = null!; 
    [Inject] public IApiService ApiService { get; set; } = null!; 

    [Parameter] public int LibraryId { get; set; }
    [Parameter] public int? TemplateId { get; set; }
    [Parameter] public WorkflowDefinition WorkflowDefinition { get; set; } = null!; 
    
    [CascadingParameter] 
    protected IMudDialogInstance MudDialog { get; set; } = null!;

    protected MudForm Form = null!;
    protected TFormModel FormModel = new();
    
    protected override async Task OnInitializedAsync()
    {
        if (TemplateId.HasValue)
        {
            Result<WorkflowTemplate> result = await ApiService.GetWorkflowTemplateAsync(TemplateId.Value);
            if (result.Succeeded)
            {
                WorkflowTemplate? template = result.Value;

                FormModel = !string.IsNullOrWhiteSpace(template.AssociationDataJson)
                    ? JsonSerializer.Deserialize<TFormModel>(template.AssociationDataJson)!
                    : new TFormModel();
            }
        }
    }
    
    protected virtual async Task SaveTemplate()
    {
        await Form.Validate();

        if (Form.IsValid)
        {
            string definitionJson = JsonSerializer.Serialize(FormModel);

            if (TemplateId.HasValue)
            {
                Dispatcher.Dispatch(new UpdateWorkflowTemplateAction(
                    TemplateId.Value,
                    LibraryId,
                    FormModel.TemplateName,
                    WorkflowDefinition.Name,
                    WorkflowDefinition.Id,
                    definitionJson
                    ));
            }
            else
            {
                Dispatcher.Dispatch(new CreateWorkflowTemplateAction(
                    FormModel.TemplateName,
                    LibraryId,
                    WorkflowDefinition.Name,
                    WorkflowDefinition.Id,
                    definitionJson
                ));
            }

            MudDialog.Close(DialogResult.Ok(true));
        }
    }

    protected virtual void Cancel()
    {
        MudDialog.Cancel();
    }
}