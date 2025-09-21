using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.WorkflowTemplates;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Api.Controllers;

/// <summary>
/// API Requires client credentials token from SharePoint Gateway.
/// End user authentication is handled by the gateway.
/// </summary>
[ApiController]
[Route("api/workflow-templates")]
[Authorize(Policy = "TrustedApp")]
public class WorkflowTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{templateId:int}")]
    public async Task<ActionResult<WorkflowTemplateResponse>> GetWorkflowTemplate(int templateId, CancellationToken ct)
    {
        Result<WorkflowTemplateDto> result = await mediator.Send(new GetWorkflowTemplateQuery((WorkflowTemplateId)templateId), ct);
        
        return this.ToActionResult(result, Mapper.MapToWorkflowTemplateResponse);
    }

    [HttpPost("")]
    public async Task<ActionResult<CreateWorkflowTemplateResponse>> CreateWorkflowTemplate([FromBody] WorkflowTemplateCreateRequest request, CancellationToken ct)
    {
        var cmd = new CreateWorkflowTemplateCommand
        {
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            LibraryId = (LibraryId)request.LibraryId,
            AssociationDataJson = request.AssociationDataJson
        };

        WorkflowTemplateId templateId = await mediator.Send(cmd, ct);

        return CreatedAtAction(nameof(CreateWorkflowTemplate), new CreateWorkflowTemplateResponse(templateId.Value));
    }

    [HttpPost("{templateId:int}")]
    public async Task<ActionResult> UpdateWorkflowTemplate(int templateId, [FromBody] WorkflowTemplateUpdateRequest request, CancellationToken ct)
    {
        var cmd = new UpdateWorkflowTemplateCommand
        {
            TemplateId = (WorkflowTemplateId)templateId,
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson
        };

        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }

    [HttpDelete("{templateId:int}")]
    public async Task<ActionResult> DeleteWorkflowTemplate(int templateId, CancellationToken ct)
    {
        var cmd = new DeleteWorkflowTemplateCommand((WorkflowTemplateId)templateId);
        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }
}