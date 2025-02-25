using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Shared.Models.Libraries;

namespace Uptime.WorkflowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/{libraryId:int}")]
    public class LibrariesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("")]
        public async Task<ActionResult<LibraryResponse>> GetLibrary(int libraryId)
        {
            var query = new GetLibraryQuery((LibraryId)libraryId);

            LibraryDto? library = await mediator.Send(query);
            if (library == null)
            {
                return NotFound($"Library with ID '{libraryId}' was not found.");
            }

            return Ok(new LibraryResponse(library.Id, library.Name));
        }

        [HttpGet("documents")]
        public async Task<ActionResult<List<LibraryDocumentResponse>>> GetDocuments(int libraryId)
        {
            var query = new GetLibraryDocumentsQuery((LibraryId)libraryId);
            List<LibraryDocumentDto> documents = await mediator.Send(query);
            
            return Ok(Mapper.MapToLibraryDocumentResponse(documents));
        }

        [HttpGet("workflow-templates")]
        public async Task<ActionResult<List<LibraryWorkflowTemplateResponse>>> GetWorkflowTemplates(int libraryId)
        {
            var query = new GetLibraryWorkflowTemplatesQuery((LibraryId)libraryId);
            List<LibraryWorkflowTemplateDto> templates = await mediator.Send(query);
            
            return Ok(Mapper.MapToLibraryWorkflowTemplateResponse(templates));
        }
    }
}