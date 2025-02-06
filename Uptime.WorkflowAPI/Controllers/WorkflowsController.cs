using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Workflows;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{workflowId:int}")]
    public async Task<ActionResult<WorkflowResponse>> GetWorkflow(int workflowId)
    {
        var query = new GetWorkflowQuery(workflowId);
        WorkflowDto? workflow = await mediator.Send(query);
        
        if (workflow == null)
        {
            return NotFound($"No workflow found for ID {workflowId}.");
        }

        return Ok(Mapper.MapToWorkflowResponse(workflow));
    }

    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<List<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, [FromQuery] WorkflowTaskStatus? status = null)
    {
        var query = new GetWorkflowTasksQuery(workflowId, status);

        List<WorkflowTaskDto> tasks = await mediator.Send(query);
        if (!tasks.Any())
        {
            return NotFound($"No tasks found for workflow ID {workflowId}.");
        }
        return Ok(Mapper.MapToWorkflowTasksResponse(tasks));
    }
    
    [HttpPost("start-workflow")]
    public async Task<ActionResult<Task<WorkflowStatus>>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        StartWorkflowCommand query = Mapper.MapToStartWorkflowCommand(request);
        WorkflowStatus status = await mediator.Send(query);

        if (status == WorkflowStatus.Invalid)
            return BadRequest("Invalid workflow type.");

        return Ok(status);
    }

    [HttpPost("{workflowId:int}/cancel-workflow")]
    public async Task<ActionResult> CancelWorkflow(int workflowId)
    {
        // TODO: add implementation

        await Task.Delay(2000);

        return NoContent();
    }
    
    [HttpPost("{workflowId:int}/terminate-workflow")]
    public async Task<ActionResult> TerminateWorkflow(int workflowId)
    {
        // TODO: add implementation

        await Task.Delay(2000);

        return NoContent();
    }
}