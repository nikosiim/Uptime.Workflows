using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Uptime.Shared.Models.WorkflowTemplates;
using Uptime.Web.Application.Models;
using Uptime.Web.Application.Queries;

namespace Uptime.Web.Presentation.Forms;

public class WorkflowInitForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Parameter] public int DocumentId { get; set; }
    [Parameter] public int TemplateId { get; set; }

    [Inject] protected IMediator Mediator { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected MudForm Form = null!;
    protected TFormModel FormModel = new();

    protected override async Task OnInitializedAsync()
    {
        WorkflowTemplateResponse? template = await Mediator.Send(new GetWorkflowTemplateQuery(TemplateId));

        FormModel = !string.IsNullOrWhiteSpace(template?.AssociationDataJson)
            ? JsonSerializer.Deserialize<TFormModel>(template.AssociationDataJson)!
            : new TFormModel();
    }

    protected virtual async Task StartWorkflow()
    {
        await Form.Validate();
        if (!Form.IsValid)
            return;
    }

    protected void Cancel()
    {
        Navigation.NavigateTo("/contracts");
    }
}