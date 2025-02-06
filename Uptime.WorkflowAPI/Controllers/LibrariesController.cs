using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
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
            var query = new GetLibraryQuery(libraryId);

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
            var query = new GetLibraryDocumentsQuery(libraryId);
            List<LibraryDocumentDto> documents = await mediator.Send(query);

            if (documents.Count == 0)
            {
                return NotFound($"No documents were found for the library with ID '{libraryId}'.");
            }
            
            return Ok(Mapper.MapToLibraryDocumentResponse(documents));
        }

        [HttpGet("workflow-templates")]
        public async Task<ActionResult<List<LibraryWorkflowTemplateResponse>>> GetWorkflowTemplates(int libraryId)
        {
            var query = new GetLibraryWorkflowTemplatesQuery(libraryId);
            List<LibraryWorkflowTemplateDto> templates = await mediator.Send(query);

            if (templates.Count == 0)
            {
                return NotFound($"No workflow templates found for library with ID {libraryId}.");
            }

            return Ok(Mapper.MapToLibraryWorkflowTemplateResponse(templates));
        }
    }
}