using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Workflows.Api.Contracts;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Api.Controllers;

[ApiController]
[Route("api/[controller]/{documentId:int}")]
[Authorize]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("workflows")]
    public async Task<ActionResult<List<DocumentWorkflowsResponse>>> GetDocumentWorkflows(int documentId, CancellationToken ct)
    {
        var query = new GetDocumentWorkflowsQuery((DocumentId)documentId);
        List<DocumentWorkflowDto> items = await mediator.Send(query, ct);

        List<DocumentWorkflowsResponse> result = items.Select(dto => new DocumentWorkflowsResponse
        {
            Id = dto.Id,
            TemplateId = dto.TemplateId,
            WorkflowTemplateName = dto.WorkflowTemplateName,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Outcome = dto.Outcome,
            IsActive = dto.IsActive
        }).ToList();
        
        return Ok(result);
    }

    [HttpGet("workflow-tasks")]
    public async Task<ActionResult<List<DocumentTasksResponse>>> GetDocumentTasks(int documentId, CancellationToken ct)
    {
        var query = new GetDocumentWorkflowTasksQuery((DocumentId)documentId);
        Result<List<DocumentWorkflowTaskDto>> result = await mediator.Send(query, ct);
        
        return this.ToActionResult(result, Mapper.MapToDocumentTasksResponse);
    }
}