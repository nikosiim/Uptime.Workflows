using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IAlterTaskPayload
{
    TaskId TaskId { get; }
    WorkflowId WorkflowId { get; }
    Dictionary<string, string?> Storage { get; }
}