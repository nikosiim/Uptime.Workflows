using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uptime.Workflows.Api.Contracts;
using Uptime.Workflows.Api.Extensions;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.DTOs;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Application.Queries;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Api.Controllers;

/// <summary>
/// API Requires client credentials token from SharePoint Gateway.
/// End user authentication is handled by the gateway.
/// </summary>
[ApiController]
[Route("api/workflow-tasks/{taskGuid:guid}")]
[Authorize(Policy = "TrustedApp")]
public class WorkflowTasksController(ISender mediator) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetTask(Guid taskGuid, CancellationToken ct)
    {
        Result<WorkflowTaskDetailsDto> result = await mediator.Send(new GetWorkflowTaskQuery(taskGuid), ct);
        return this.ToActionResult(result, Mapper.MapToWorkflowTaskResponse);
    }

    [HttpPost("update")]
    public async Task<ActionResult> AlterTask(Guid taskGuid, [FromBody] AlterTaskRequest request, CancellationToken ct)
    {
        var cmd = new AlterTaskCommand
        {
            TaskGuid = taskGuid,
            ExecutorSid = (PrincipalSid)request.ExecutorSid,
            Action = EnumMapper.MapToDomain(request.Action),
            Payload = request.Input
        }; 
        
        Result<Unit> result = await mediator.Send(cmd, ct);

        return this.ToActionResult(result);
    }
}