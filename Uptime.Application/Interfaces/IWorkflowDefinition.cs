using Uptime.Application.Common;
using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

/// <summary>
/// Base interface for all workflow definitions.
/// Implementing classes should provide metadata about their workflow.
/// </summary>
public interface IWorkflowDefinition
{
    string WorkflowBaseId { get; }
    WorkflowConfiguration Configuration { get; }
    WorkflowDefinition GetDefinition();
}