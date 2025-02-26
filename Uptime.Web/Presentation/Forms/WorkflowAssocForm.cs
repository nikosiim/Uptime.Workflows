using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;
using Uptime.Web.Application.Commands;
using Uptime.Web.Application.Definitions;
using Uptime.Web.Application.DTOs;
using Uptime.Web.Application.Models;
using Uptime.Web.Application.Queries;
namespace Uptime.Web.Presentation.Forms;

public abstract class WorkflowAssocForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Parameter] public int LibraryId { get; set; }
    [Parameter] public int? TemplateId { get; set; }
    [Parameter] public IWorkflowDefinition WorkflowDefinition { get; set; } = null!; 
    
    [Inject] protected IMediator Mediator { get; set; } = null!;

    [CascadingParameter] 
    protected IMudDialogInstance MudDialog { get; set; } = null!;

    protected MudForm Form = null!;
    protected TFormModel FormModel = new();
    
    protected override async Task OnInitializedAsync()
    {
        if (TemplateId.HasValue)
        {
            WorkflowTemplate template = await Mediator.Send(new GetWorkflowTemplateQuery(TemplateId.Value));

            FormModel = !string.IsNullOrWhiteSpace(template.AssociationDataJson)
                ? JsonSerializer.Deserialize<TFormModel>(template.AssociationDataJson)!
                : new TFormModel();
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
                await Mediator.Send(new UpdateWorkflowTemplateCommand
                {
                    TemplateId = TemplateId.Value,
                    TemplateName = FormModel.TemplateName,
                    WorkflowName = WorkflowDefinition.Name,
                    WorkflowBaseId = WorkflowDefinition.Id,
                    AssociationDataJson = definitionJson
                });
            }
            else
            {
                await Mediator.Send(new CreateWorkflowTemplateCommand
                {
                    LibraryId = LibraryId,
                    TemplateName = FormModel.TemplateName,
                    WorkflowName = WorkflowDefinition.Name,
                    WorkflowBaseId = WorkflowDefinition.Id,
                    AssociationDataJson = definitionJson
                });
            }

            MudDialog.Close(DialogResult.Ok(true));
        }
    }

    protected virtual void Cancel()
    {
        MudDialog.Cancel();
    }
}