using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.Common;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Domain.Common;
using Uptime.Domain.Enums;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Tasks;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-tasks/{taskId:int}")]
public class WorkflowTasksController(IMediator mediator) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetTask(int taskId)
    {
        WorkflowTaskDto? task = await mediator.Send(new GetWorkflowTaskQuery((TaskId)taskId));
        if (task == null)
        {
            return NotFound($"No task found with ID {taskId}.");
        }

        return Ok(Mapper.MapToWorkflowTaskResponse(task));
    }

    [HttpPost("update")]
    public async Task<ActionResult<WorkflowStatus>> AlterTask(int taskId, [FromBody] AlterTaskRequest request)
    {
        AlterTaskCommand command = Mapper.MapToAlterTaskCommand(request, taskId);
        WorkflowPhase result = await mediator.Send(command);

        return Ok(result.MapToWorkflowStatus());
    }
}