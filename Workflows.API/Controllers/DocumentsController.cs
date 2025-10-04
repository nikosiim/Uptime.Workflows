using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflows.Api.Contracts;
using Workflows.Api.Extensions;
using Workflows.Application.DTOs;
using Workflows.Application.Messaging;
using Workflows.Application.Queries;
using Workflows.Core.Common;

namespace Workflows.Api.Controllers;

/// <summary>
/// API Requires client credentials token from SharePoint Gateway.
/// End user authentication is handled by the gateway.
/// </summary>
[ApiController]
[Route("api/[controller]/{documentId:int}")]
[Authorize(Policy = "TrustedApp")]
public class DocumentsController(ISender mediator) : ControllerBase
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