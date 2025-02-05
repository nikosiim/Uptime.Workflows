﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Queries;
using Uptime.Shared.Enums;
using Uptime.Shared.Models.Workflows;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(IWorkflowService workflowService, IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("{workflowId:int}")]
    public async Task<ActionResult<WorkflowResponse>> GetWorkflow(int workflowId)
    {
        var query = new GetWorkflowQuery(workflowId);
        WorkflowDto? workflow = await mediator.Send(query);
        
        if (workflow == null)
        {
            return NotFound($"No workflow found for ID {workflowId}.");
        }

        return Ok(mapper.Map<WorkflowResponse>(workflow));
    }

    [HttpGet("{workflowId:int}/workflow-tasks")]
    public async Task<ActionResult<List<WorkflowTasksResponse>>> GetWorkflowTasks(int workflowId, [FromQuery] WorkflowTaskStatus? status = null)
    {
        var query = new GetWorkflowTasksQuery(workflowId, status);

        List<WorkflowTaskDto> tasks = await mediator.Send(query);
        if (!tasks.Any())
        {
            return NotFound($"No tasks found for workflow ID {workflowId}.");
        }
        return Ok(mapper.Map<List<WorkflowTasksResponse>>(tasks));
    }

    // TODO: Start Workflow
    // TODO: Cancel Workflow
    // TODO: Terminate Workflow

    [HttpPost("start-approval-workflow")]
    public async Task<ActionResult> StartApprovalWorkflow([FromBody] ApprovalWorkflowPayload payload)
    {
        bool result = await workflowService.StartWorkflowAsync(payload);

        return Ok(result);
    }

    [HttpPost("complete-approval-task")]
    public async Task<ActionResult> CompleteApprovalTask([FromBody] AlterTaskPayload payload)
    {
        bool result = await workflowService.UpdateWorkflowTaskAsync(payload);

        return Ok(result);
    }
}