﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.Tasks;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Api.Controllers;

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
        var cmd = new AlterTaskCommand(User, (TaskId)taskId, request.Input);
        Result<Unit> result = await mediator.Send(cmd);

        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}