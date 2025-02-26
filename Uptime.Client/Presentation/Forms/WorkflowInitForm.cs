using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Queries;

namespace Uptime.Client.Presentation.Forms;

public class WorkflowInitForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] public IMediator Mediator { get; set; } = null!; 
    [Inject] public ISnackbar Snackbar { get; set; } = null!; 
    [Parameter] public int DocumentId { get; set; }
    [Parameter] public int TemplateId { get; set; }

    protected MudForm Form = null!;
    protected TFormModel FormModel = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new GetWorkflowTemplateQuery(TemplateId));
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