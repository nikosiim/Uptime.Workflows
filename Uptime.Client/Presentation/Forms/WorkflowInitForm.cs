using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;

namespace Uptime.Client.Presentation.Forms;

public class WorkflowInitForm<TFormModel> : ComponentBase where TFormModel : IWorkflowFormModel, new()
{
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] public IApiService ApiService { get; set; } = null!; 
    [Inject] public ISnackbar Snackbar { get; set; } = null!; 
    [Parameter] public int DocumentId { get; set; }
    [Parameter] public int TemplateId { get; set; }

    protected MudForm Form = null!;
    protected TFormModel FormModel = new();

    protected override async Task OnInitializedAsync()
    {
        Result<WorkflowTemplate> result = await ApiService.GetWorkflowTemplateAsync(TemplateId);
        if (result.Succeeded)
        {
            WorkflowTemplate? template = result.Value;

            FormModel = !string.IsNullOrWhiteSpace(template.AssociationDataJson)
                ? JsonSerializer.Deserialize<TFormModel>(template.AssociationDataJson)!
                : new TFormModel();
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