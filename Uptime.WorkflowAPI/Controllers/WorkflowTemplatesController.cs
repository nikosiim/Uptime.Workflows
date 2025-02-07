using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-templates")]
public class WorkflowTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{templateId:int}")]
    public async Task<ActionResult<WorkflowTemplateResponse>> GetWorkflowTemplate(int templateId)
    {
        var query = new GetWorkflowTemplateQuery((WorkflowTemplateId)templateId);
        WorkflowTemplateDto? template = await mediator.Send(query);

        if (template == null || template.Id == 0) 
        {
            return NotFound($"Workflow template with ID '{templateId}' was not found.");
        }

        return Ok(Mapper.MapToWorkflowTemplateResponse(template));
    }

    [HttpPost("")]
    public async Task<ActionResult<CreateWorkflowTemplateResponse>> CreateWorkflowTemplate([FromBody] WorkflowTemplateCreateRequest request)
    {
        CreateWorkflowTemplateCommand command = Mapper.MapToCreateWorkflowTemplateCommand(request);
        WorkflowTemplateId templateId = await mediator.Send(command);

        return CreatedAtAction(nameof(CreateWorkflowTemplate), new CreateWorkflowTemplateResponse(templateId.Value));
    }

    [HttpPost("{templateId:int}")]
    public async Task<ActionResult> UpdateWorkflowTemplate(int templateId, [FromBody] WorkflowTemplateUpdateRequest request)
    {
        UpdateWorkflowTemplateCommand command = Mapper.MapToUpdateWorkflowTemplateCommand(request, templateId);
        bool result = await mediator.Send(command);

        if (!result)
        {
            return NotFound($"Workflow template with ID '{templateId}' was not found.");
        }

        return NoContent();
    }

    [HttpDelete("{templateId:int}")]
    public async Task<ActionResult> DeleteWorkflowTemplate(int templateId)
    {
        var command = new DeleteWorkflowTemplateCommand((WorkflowTemplateId)templateId);
        bool result = await mediator.Send(command);

        if (!result)
        {
            return NotFound($"Workflow template with ID '{templateId}' was not found.");
        }

        return NoContent();
    }
}