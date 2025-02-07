using Uptime.Application.Interfaces;
using Uptime.Domain.Common;

namespace Uptime.Application.Common;

public record AlterTaskPayload(TaskId TaskId, WorkflowId WorkflowId, Dictionary<string, string?> Storage) : IAlterTaskPayload;