using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Workflows.Core.Common;
using Uptime.Shared.Models.Tasks;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-tasks/{taskId:int}")]
public class WorkflowTasksController(IMediator mediator) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetTask(int taskId)
    {
        WorkflowTaskDetailsDto? task = await mediator.Send(new GetWorkflowTaskQuery((TaskId)taskId));
        if (task == null)
        {
            return NotFound($"No task found with ID {taskId}.");
        }

        return Ok(Mapper.MapToWorkflowTaskResponse(task));
    }

    [HttpPost("update")]
    public async Task<ActionResult> AlterTask(int taskId, [FromBody] AlterTaskRequest request)
    {
        AlterTaskCommand command = Mapper.MapToAlterTaskCommand(request, taskId);
        Result<Unit> result = await mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}