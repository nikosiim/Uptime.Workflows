using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Shared.Models.Documents;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]/{documentId:int}")]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("workflows")]
    public async Task<ActionResult<List<DocumentWorkflowsResponse>>> GetDocumentWorkflows(int documentId)
    {
        var query = new GetDocumentWorkflowsQuery((DocumentId)documentId);
        List<DocumentWorkflowDto> workflows = await mediator.Send(query);

        if (workflows.Count == 0)
        {
            return NotFound($"No workflow instances found for document ID {documentId}.");
        }

        return Ok(Mapper.MapToDocumentWorkflowsResponse(workflows));
    }

    [HttpGet("workflow-tasks")]
    public async Task<ActionResult<List<DocumentTasksResponse>>> GetDocumentTasks(int documentId)
    {
        var query = new GetDocumentWorkflowTasksQuery((DocumentId)documentId);
        List<DocumentWorkflowTaskDto> tasks = await mediator.Send(query);

        if (tasks.Count == 0)
        {
            return NotFound($"No tasks found for document ID {documentId}.");
        }
        
        return Ok(Mapper.MapToDocumentTasksResponse(tasks));
    }
}