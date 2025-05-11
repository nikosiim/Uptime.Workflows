using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Shared.Models.Tasks;
using Uptime.Workflows.Api.Extensions;
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
    public async Task<ActionResult<WorkflowTaskResponse>> GetTask(int taskId, CancellationToken ct)
    {
        Result<WorkflowTaskDetailsDto> result = await mediator.Send(new GetWorkflowTaskQuery((TaskId)taskId), ct);
        return this.ToActionResult(result, Mapper.MapToWorkflowTaskResponse);
    }

    [HttpPost("update")]
    public async Task<ActionResult> AlterTask(int taskId, [FromBody] AlterTaskRequest request, CancellationToken ct)
    {
        var cmd = new AlterTaskCommand(User, (TaskId)taskId, request.Input);
        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }
}