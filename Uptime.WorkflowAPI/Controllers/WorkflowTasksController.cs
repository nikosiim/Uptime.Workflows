using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Models.Common;
using Uptime.Application.Queries;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Tasks;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-tasks/{taskId:int}")]
public class WorkflowTasksController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetTask(int taskId)
    {
        WorkflowTaskDto? task = await mediator.Send(new GetWorkflowTaskQuery(taskId));
        if (task == null)
        {
            return NotFound($"No task found with ID {taskId}.");
        }

        return Ok(mapper.Map<WorkflowTaskResponse>(task));
    }

    [HttpPost("update")]
    public async Task<ActionResult<WorkflowStatus>> UpdateTask(int taskId, [FromBody] TaskUpdateRequest request)
    {
        var payload = new AlterTaskPayload(taskId, request.WorkflowId, request.Storage);

        var query = new AlterTaskCommand(payload);
        WorkflowStatus result = await mediator.Send(query);

        return Ok(result);
    }
}