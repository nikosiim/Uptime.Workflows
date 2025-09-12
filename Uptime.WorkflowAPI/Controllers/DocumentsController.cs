using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.Documents;
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
        List<DocumentWorkflowDto> workflows = await mediator.Send(query, ct);
        
        return Ok(Mapper.MapToDocumentWorkflowsResponse(workflows));
    }

    [HttpGet("workflow-tasks")]
    public async Task<ActionResult<List<DocumentTasksResponse>>> GetDocumentTasks(int documentId, CancellationToken ct)
    {
        var query = new GetDocumentWorkflowTasksQuery((DocumentId)documentId);
        Result<List<DocumentWorkflowTaskDto>> result = await mediator.Send(query, ct);

        return this.ToActionResult(result, Mapper.MapToDocumentTasksResponse);
    }
}