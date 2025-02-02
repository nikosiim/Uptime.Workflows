﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-templates")]
public class WorkflowTemplatesController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("{templateId:int}")]
    public async Task<ActionResult<WorkflowTemplateResponse>> GetWorkflowTemplate(int templateId)
    {
        var query = new GetWorkflowTemplateQuery(templateId);

        WorkflowTemplateDto? template = await mediator.Send(query);
        if (template == null || template.Id == 0) 
        {
            return NotFound($"Workflow template with ID '{templateId}' was not found.");
        }

        return Ok(mapper.Map<WorkflowTemplateResponse>(template));
    }

    [HttpPost("")]
    public async Task<ActionResult<CreateWorkflowTemplateResponse>> CreateWorkflowTemplate([FromBody] WorkflowTemplateCreateRequest request)
    {
        var command = mapper.Map<CreateWorkflowTemplateCommand>(request);
        int templateId = await mediator.Send(command);

        return CreatedAtAction(nameof(CreateWorkflowTemplate), new CreateWorkflowTemplateResponse(templateId));
    }

    [HttpPost("{templateId:int}")]
    public async Task<ActionResult> UpdateWorkflowTemplate(int templateId, [FromBody] WorkflowTemplateUpdateRequest request)
    {
        var command = new UpdateWorkflowTemplateCommand
        {
            TemplateId = templateId,
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            AssociationDataJson = request.AssociationDataJson
        };

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
        var command = new DeleteWorkflowTemplateCommand(templateId);
        bool result = await mediator.Send(command);

        if (!result)
        {
            return NotFound($"Workflow template with ID '{templateId}' was not found.");
        }

        return NoContent();
    }
}