using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
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
        WorkflowTaskDto? task = await mediator.Send(new GetWorkflowTaskQuery(taskId));
        if (task == null)
        {
            return NotFound($"No task found with ID {taskId}.");
        }

        return Ok(Mapper.MapToWorkflowTaskResponse(task));
    }

    [HttpPost("update")]
    public async Task<ActionResult<WorkflowStatus>> AlterTask(int taskId, [FromBody] AlterTaskRequest request)
    {
        AlterTaskCommand command = Mapper.MapToAlterTaskCommand(request , taskId);
        WorkflowStatus result = await mediator.Send(command);

        return Ok(result);
    }
}