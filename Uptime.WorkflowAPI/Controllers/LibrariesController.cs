using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Workflows.Api.Contracts;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/{libraryId:int}")]
    [Authorize(Policy = "UserOrAdmin")]
    public class LibrariesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("")]
        public async Task<ActionResult<LibraryResponse>> GetLibrary(int libraryId, CancellationToken ct)
        {
            var query = new GetLibraryQuery((LibraryId)libraryId);
            Result<LibraryDto> result = await mediator.Send(query, ct);
           
            return this.ToActionResult(result, dto => new LibraryResponse(dto.Id, dto.Name));
        }

        [HttpGet("documents")]
        public async Task<ActionResult<List<LibraryDocumentResponse>>> GetDocuments(int libraryId, CancellationToken ct)
        {
            var query = new GetLibraryDocumentsQuery((LibraryId)libraryId);
            List<LibraryDocumentDto> items = await mediator.Send(query, ct);

            List<LibraryDocumentResponse> result = items.Select(dto => new LibraryDocumentResponse
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                LibraryId = dto.LibraryId
            }).ToList();
            
            return Ok(result);
        }

        [HttpGet("workflow-templates")]
        public async Task<ActionResult<List<LibraryWorkflowTemplateResponse>>> GetWorkflowTemplates(int libraryId, CancellationToken ct)
        {
            var query = new GetLibraryWorkflowTemplatesQuery((LibraryId)libraryId);
            List<LibraryWorkflowTemplateDto> templates = await mediator.Send(query, ct);

            List<LibraryWorkflowTemplateResponse> result = templates.Select(dto => new LibraryWorkflowTemplateResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                WorkflowBaseId = dto.WorkflowBaseId,
                AssociationDataJson = dto.AssociationDataJson,
                Created = dto.Created
            }).ToList();
            
            return Ok(result);
        }
    }
}