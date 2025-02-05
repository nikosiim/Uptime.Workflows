using Uptime.Application.Interfaces;

namespace Uptime.Application.Models.Common;

public record AlterTaskPayload(int TaskId, int WorkflowId, Dictionary<string, object> Storage) : IAlterTaskPayload;