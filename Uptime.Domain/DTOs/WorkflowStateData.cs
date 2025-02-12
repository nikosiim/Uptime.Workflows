using Uptime.Domain.Enums;
using Uptime.Domain.Interfaces;

namespace Uptime.Domain.DTOs;

public class WorkflowStateData<TContext>
    where TContext : IWorkflowContext, new()
{
    public WorkflowPhase Phase { get; set; }
    public TContext Context { get; set; } = new();
}