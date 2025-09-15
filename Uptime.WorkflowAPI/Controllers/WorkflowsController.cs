using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.Workflows;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDefinitionResponse>>> GetDefinitions(CancellationToken ct)
    {
        var query = new GetWorkflowDefinitionQuery();
        List<WorkflowDefinition> definitions = await mediator.Send(query, ct);

        return Ok(Mapper.MapToWorkflowDefinitionResponse(definitions));
    }

    [HttpGet("{workflowId:int}")]
    public async Task<ActionResult<WorkflowDetailsResponse>> GetWorkflow(int workflowId, CancellationToken ct)
    {
        var query = new GetWorkflowDetailsQuery((WorkflowId)workflowId);
        Result<WorkflowDetailsDto> result = await mediator.Send(query, ct);
      
        return this.ToActionResult(result, Mapper.MapToWorkflowDetailsResponse);
    }
    
    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, CancellationToken ct, [FromQuery] string? workflowTaskStatus = null)
    {
        var query = new GetWorkflowTasksQuery((WorkflowId)workflowId, Mapper.ToDomain(workflowTaskStatus));
        Result<List<WorkflowTaskDto>> result = await mediator.Send(query, ct);

        return this.ToActionResult(result, Mapper.MapToWorkflowTasksResponse);
    }

    [HttpGet("{workflowId:int}/workflow-histories")]
    public async Task<ActionResult<IEnumerable<WorkflowHistoryResponse>>> GetWorkflowHistory(int workflowId, CancellationToken ct)
    {
        var query = new GetWorkflowHistoryQuery((WorkflowId)workflowId);
        List<WorkflowHistoryDto> items = await mediator.Send(query, ct);
      
        return Ok(Mapper.MapToWorkflowHistoryResponse(items));
    }
    
    [HttpGet("{workflowId:int}/workflow-context")]
    public async Task<ActionResult<string>> GetModificationContext(int workflowId)
    {
        var query = new GetModificationContextQuery((WorkflowId)workflowId);
        Result<string> result = await mediator.Send(query);

        return this.ToActionResult(result);
    }
    
    [HttpPost("start-workflow")]
    public async Task<ActionResult> StartWorkflow([FromBody] StartWorkflowRequest request, CancellationToken ct)
    {
        var cmd = new StartWorkflowCommand
        {
            ExecutedBySid = request.InitiatorSid,
            DocumentId = (DocumentId)request.DocumentId,
            WorkflowTemplateId = (WorkflowTemplateId)request.WorkflowTemplateId,
            Storage = request.Storage
        };

        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }

    [HttpPost("{workflowId:int}/modify-workflow")]
    public async Task<ActionResult> ModifyWorkflow(int workflowId, [FromBody] ModifyWorkflowRequest request, CancellationToken ct)
    {
        var cmd = new ModifyWorkflowCommand
        {
            WorkflowId = (WorkflowId)workflowId,
            ExecutedBySid = request.ExecutorSid,
            InputContext = request.ModificationContext
        };
        
        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }

    [HttpPost("{workflowId:int}/cancel-workflow")]
    public async Task<ActionResult> CancelWorkflow(int workflowId, [FromBody] CancelWorkflowRequest request, CancellationToken ct)
    {
        var cmd = new CancelWorkflowCommand
        {
            ExecutedBySid = request.ExecutorSid,
            WorkflowId = (WorkflowId)workflowId,
            Comment = request.Comment
        };

        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }

    [HttpDelete("{workflowId:int}/delete-workflow")]
    public async Task<ActionResult> DeleteWorkflow(int workflowId, CancellationToken ct)
    {
        var cmd = new DeleteWorkflowCommand((WorkflowId)workflowId);
        await mediator.Send(cmd, ct);

        return NoContent();
    }
}