using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Shared.Models.Workflows;
using WorkflowTaskStatus = Uptime.Shared.Enums.WorkflowTaskStatus;

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

        return Ok(definitions);
    }

    [HttpGet("{workflowId:int}")]
    public async Task<ActionResult<WorkflowResponse>> GetWorkflow(int workflowId)
    {
        var query = new GetWorkflowQuery((WorkflowId)workflowId);
        WorkflowDto? workflow = await mediator.Send(query);
        
        if (workflow == null)
        {
            return NotFound($"No workflow found for ID {workflowId}.");
        }

        return Ok(Mapper.MapToWorkflowResponse(workflow));
    }

    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, [FromQuery] WorkflowTaskStatus? status = null)
    {
        var query = new GetWorkflowTasksQuery((WorkflowId)workflowId, status.ToDomain());

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
    
    [HttpPost("start-workflow")]
    public async Task<ActionResult<Task>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        StartWorkflowCommand cmd = Mapper.MapToStartWorkflowCommand(request);
        string phase = await mediator.Send(cmd);

        if (phase == BaseState.Invalid.Value)
            return BadRequest("Failed to start workflow.");

        return Ok();
    }

    [HttpPost("{workflowId:int}/cancel-workflow")]
    public async Task<ActionResult> CancelWorkflow(int workflowId)
    {
        var cmd = new CancelWorkflowCommand((WorkflowId)workflowId);
        await mediator.Send(cmd);

        return NoContent();
    }

    [HttpPost("{workflowId:int}/delete-workflow")]
    public async Task<ActionResult> DeleteWorkflow(int workflowId)
    {
        var cmd = new DeleteWorkflowCommand((WorkflowId)workflowId);
        await mediator.Send(cmd);

        return NoContent();
    }
}