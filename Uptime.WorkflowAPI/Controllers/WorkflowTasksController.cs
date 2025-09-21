using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Workflows.Api.Contracts;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace Uptime.Workflows.Api.Controllers;

/// <summary>
/// API Requires client credentials token from SharePoint Gateway.
/// End user authentication is handled by the gateway.
/// </summary>
[ApiController]
[Route("api/workflow-tasks/{taskId:int}")]
[Authorize(Policy = "TrustedApp")]
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
        var cmd = new AlterTaskCommand
        {
            TaskId = (TaskId)taskId,
            ExecutorSid = request.ExecutorSid,
            Action = EnumMapper.MapToDomain(request.Action),
            Payload = request.Input
        }; 
        
        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }
}