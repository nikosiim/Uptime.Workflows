using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Workflows.Core.Common;
using Uptime.Shared.Models.Workflows;
using Uptime.Workflows.Core;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDefinitionResponse>>> GetDefinitions()
    {
        var query = new GetWorkflowDefinitionQuery();
        List<WorkflowDefinition> definitions = await mediator.Send(query);

        return Ok(Mapper.MapToWorkflowDefinitionResponse(definitions));
    }

    [HttpGet("{workflowId:int}")]
    public async Task<ActionResult<WorkflowDetailsResponse>> GetWorkflow(int workflowId)
    {
        var query = new GetWorkflowDetailsQuery((WorkflowId)workflowId);
        WorkflowDetailsDto? workflow = await mediator.Send(query);
        
        if (workflow == null)
        {
            return NotFound($"No workflow found for ID {workflowId}.");
        }

        return Ok(Mapper.MapToWorkflowDetailsResponse(workflow));
    }
    
    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, [FromQuery] string? workflowTaskStatus = null)
    {
        var query = new GetWorkflowTasksQuery((WorkflowId)workflowId, Mapper.ToDomain(workflowTaskStatus));

        List<WorkflowTaskDto> tasks = await mediator.Send(query);
        if (tasks.Count == 0)
        {
            return NotFound($"No tasks found for workflow ID {workflowId}.");
        }

        return Ok(Mapper.MapToWorkflowTasksResponse(tasks));
    }

    [HttpGet("{workflowId:int}/workflow-histories")]
    public async Task<ActionResult<IEnumerable<WorkflowHistoryResponse>>> GetWorkflowHistory(int workflowId)
    {
        var query = new GetWorkflowHistoryQuery((WorkflowId)workflowId);
        List<WorkflowHistoryDto> items = await mediator.Send(query);
      
        return Ok(Mapper.MapToWorkflowHistoryResponse(items));
    }
    
    [HttpGet("{workflowId:int}/workflow-context")]
    public async Task<ActionResult<string>> GetModificationContext(int workflowId)
    {
        var query = new GetModificationContextQuery((WorkflowId)workflowId);
        Result<string> result = await mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    
    [HttpPost("start-workflow")]
    public async Task<ActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        StartWorkflowCommand cmd = Mapper.MapToStartWorkflowCommand(request);
        Result<Unit> result = await mediator.Send(cmd);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{workflowId:int}/modify-workflow")]
    public async Task<ActionResult> ModifyWorkflow(int workflowId, [FromBody] ModifyWorkflowRequest request)
    {
        ModificationPayload payload = Mapper.MapToModificationPayload(request);

        var cmd = new ModifyWorkflowCommand((WorkflowId)workflowId, payload);
        Result<Unit> result = await mediator.Send(cmd);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{workflowId:int}/cancel-workflow")]
    public async Task<ActionResult> CancelWorkflow(int workflowId, [FromBody] CancelWorkflowRequest request)
    {
        var cmd = new CancelWorkflowCommand((WorkflowId)workflowId, request.Executor, request.Comment);
        Result<Unit> result = await mediator.Send(cmd);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{workflowId:int}/delete-workflow")]
    public async Task<ActionResult> DeleteWorkflow(int workflowId)
    {
        var cmd = new DeleteWorkflowCommand((WorkflowId)workflowId);
        await mediator.Send(cmd);

        return NoContent();
    }
}