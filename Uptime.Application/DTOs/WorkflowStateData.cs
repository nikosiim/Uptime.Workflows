using Uptime.Application.Interfaces;
using Uptime.Domain.Enums;

namespace Uptime.Application.DTOs;

public class WorkflowStateData<TContext>
    where TContext : IWorkflowContext, new()
{
    public WorkflowPhase Phase { get; set; }
    public TContext Context { get; set; } = new();
}