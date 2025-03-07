using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;
using Fluxor;
using MediatR;
using Uptime.Client.Application.Commands;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Client.Application.Queries;

namespace Uptime.Client.Presentation.Forms;

public abstract class WorkflowAssocForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Inject] public IDispatcher Dispatcher { get; set; } = null!; 
    [Inject] public IMediator Mediator { get; set; } = null!; 
    [Inject] public ISnackbar Snackbar { get; set; } = null!; 

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
            Result<WorkflowTemplate> result = await Mediator.Send(new GetWorkflowTemplateQuery(TemplateId.Value));
            if (result.Succeeded)
            {
                FormModel = !string.IsNullOrWhiteSpace(result.Value.AssociationDataJson)
                    ? JsonSerializer.Deserialize<TFormModel>(result.Value.AssociationDataJson)!
                    : new TFormModel();
            }
            else
            {
                Snackbar.Add("Töövoo malli laadimine ebaõnnestus", Severity.Error);
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
                Result<bool> result = await Mediator.Send(new UpdateWorkflowTemplateCommand
                {
                    TemplateId = TemplateId.Value,
                    TemplateName = FormModel.TemplateName,
                    WorkflowName = WorkflowDefinition.Name,
                    WorkflowBaseId = WorkflowDefinition.Id,
                    AssociationDataJson = definitionJson
                });

                MudDialog.Close(DialogResult.Ok(result.Succeeded));
            }
            else
            {
                Result<int> result = await Mediator.Send(new CreateWorkflowTemplateCommand
                {
                    LibraryId = LibraryId,
                    TemplateName = FormModel.TemplateName,
                    WorkflowName = WorkflowDefinition.Name,
                    WorkflowBaseId = WorkflowDefinition.Id,
                    AssociationDataJson = definitionJson
                });

                MudDialog.Close(DialogResult.Ok(result.Succeeded));
            }
        }
    }

    protected virtual void Cancel()
    {
        MudDialog.Cancel();
    }
}