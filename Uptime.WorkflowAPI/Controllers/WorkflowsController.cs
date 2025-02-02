using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Queries;
using Uptime.Shared.Models.Workflows;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IWorkflowService workflowService, IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<List<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId)
    {
        var query = new GetWorkflowTasksQuery(workflowId);

        List<WorkflowTaskDto> tasks = await mediator.Send(query);
        if (!tasks.Any())
        {
            return NotFound($"No tasks found for workflow ID {workflowId}.");
        }
        return Ok(mapper.Map<List<WorkflowTasksResponse>>(tasks));
    }

    [HttpPost("start-approval-workflow")]
    public async Task<ActionResult> StartApprovalWorkflow([FromBody] ApprovalWorkflowPayload payload)
    {
        bool result = await workflowService.StartWorkflowAsync(payload);

        return Ok(result);
    }

    [HttpPost("complete-approval-task")]
    public async Task<ActionResult> CompleteApprovalTask([FromBody] TaskCompletionPayload payload)
    {
        bool result = await workflowService.CompleteTaskAsync(payload);

        return Ok(result);
    }
}