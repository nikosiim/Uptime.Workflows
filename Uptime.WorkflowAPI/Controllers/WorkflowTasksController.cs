using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Queries;
using Uptime.Shared.Models.Tasks;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-tasks/{taskId:int}")]
public class WorkflowTasksController(IWorkflowService workflowService, IMediator mediator, IMapper mapper) : ControllerBase
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
    public async Task<ActionResult> UpdateTask(int taskId, [FromBody] TaskUpdateRequest request)
    {
        //if (request.WorkflowId == null)
        //    return BadRequest("WorkflowId is required.");

        //var payload = new AlterTaskPayload
        //{
        //    WorkflowId = request.WorkflowId.Value,
        //    TaskId = taskId,
        //    Outcome = request.Outcome,
        //    Comments = request.Comments
        //};

        //bool success = await workflowService.UpdateWorkflowTaskAsync(payload);
        //if (!success)
        //{
        //    return BadRequest($"Task {taskId} could not be updated.");
        //}

        return NoContent();
    }
}