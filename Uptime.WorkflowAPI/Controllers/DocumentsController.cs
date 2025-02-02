using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Shared.Models.Documents;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]/{documentId:int}")]
public class DocumentsController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("workflows")]
    public async Task<ActionResult<List<DocumentWorkflowsResponse>>> GetDocumentWorkflows(int documentId)
    {
        var query = new GetDocumentWorkflowsQuery(documentId);

        List<DocumentWorkflowDto> workflows = await mediator.Send(query);
        if (!workflows.Any())
        {
            return NotFound($"No workflow instances found for document ID {documentId}.");
        }
        
        return Ok(mapper.Map<List<DocumentWorkflowsResponse>>(workflows));
    }

    [HttpGet("workflow-tasks")]
    public async Task<ActionResult<List<DocumentTasksResponse>>> GetDocumentTasks(int documentId)
    {
        var query = new GetDocumentWorkflowTasksQuery(documentId);

        List<DocumentWorkflowTaskDto> tasks = await mediator.Send(query);
        if (!tasks.Any())
        {
            return NotFound($"No tasks found for document ID {documentId}.");
        }
        
        return Ok(mapper.Map<List<DocumentTasksResponse>>(tasks));
    }
}