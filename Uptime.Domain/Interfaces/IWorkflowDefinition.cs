using Uptime.Domain.Common;

namespace Uptime.Domain.Interfaces;

/// <summary>
/// Base interface for all workflow definitions.
/// Implementing classes should provide metadata about their workflow.
/// </summary>
public interface IWorkflowDefinition
{
    string Id { get; }
    string Name { get; }
    string DisplayName { get; }
    WorkflowDefinition GetDefinition();
    ReplicatorConfiguration? ReplicatorConfiguration { get; }
}