using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.Libraries;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/{libraryId:int}")]
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
        [Authorize(Policy = "ApiAdminAccess")]
        public async Task<ActionResult<List<LibraryDocumentResponse>>> GetDocuments(int libraryId, CancellationToken ct)
        {
            var query = new GetLibraryDocumentsQuery((LibraryId)libraryId);
            List<LibraryDocumentDto> dtos = await mediator.Send(query, ct);
            
            return Ok(Mapper.MapToLibraryDocumentResponse(dtos));
        }

        [HttpGet("workflow-templates")]
        [Authorize]
        public async Task<ActionResult<List<LibraryWorkflowTemplateResponse>>> GetWorkflowTemplates(int libraryId, CancellationToken ct)
        {
            var query = new GetLibraryWorkflowTemplatesQuery((LibraryId)libraryId);
            List<LibraryWorkflowTemplateDto> templates = await mediator.Send(query, ct);
            
            return Ok(Mapper.MapToLibraryWorkflowTemplateResponse(templates));
        }
    }
}