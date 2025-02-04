using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Uptime.Application.DTOs;
using Uptime.Application.Queries;
using Uptime.Shared.Models.Tasks;

namespace Uptime.WorkflowAPI.Controllers;

[ApiController]
[Route("api/workflow-tasks")]
public class WorkflowTasksController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("{taskId:int}")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetWorkflowTask(int taskId)
    {
        WorkflowTaskDto? task = await mediator.Send(new GetWorkflowTaskQuery(taskId));
        if (task == null)
        {
            return NotFound($"No task found with ID {taskId}.");
        }

        return Ok(mapper.Map<WorkflowTaskResponse>(task));
    }
}