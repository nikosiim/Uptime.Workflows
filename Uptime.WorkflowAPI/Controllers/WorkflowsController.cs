using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Workflows;
using WorkflowTaskStatus = Uptime.Shared.Enums.WorkflowTaskStatus;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IMediator mediator) : ControllerBase
{
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
    public async Task<ActionResult<List<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, [FromQuery] WorkflowTaskStatus? status = null)
    {
        var query = new GetWorkflowTasksQuery((WorkflowId)workflowId, status.ToDomain());

        List<WorkflowTaskDto> tasks = await mediator.Send(query);
        if (tasks.Count == 0)
        {
            return NotFound($"No tasks found for workflow ID {workflowId}.");
        }
        return Ok(Mapper.MapToWorkflowTasksResponse(tasks));
    }
    
    [HttpPost("start-workflow")]
    public async Task<ActionResult<Task<WorkflowStatus>>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        StartWorkflowCommand cmd = Mapper.MapToStartWorkflowCommand(request);
        WorkflowPhase phase = await mediator.Send(cmd);

        if (phase == WorkflowPhase.Invalid)
            return BadRequest("Invalid workflow type.");

        return Ok(phase.MapToWorkflowStatus());
    }

    [HttpPost("{workflowId:int}/cancel-workflow")]
    public async Task<ActionResult> CancelWorkflow(int workflowId)
    {
        var cmd = new CancelWorkflowCommand((WorkflowId)workflowId);
        await mediator.Send(cmd);

        return NoContent();
    }
}