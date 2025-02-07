using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public record AlterTaskPayload(int TaskId, int WorkflowId, Dictionary<string, string?> Storage) : IAlterTaskPayload;