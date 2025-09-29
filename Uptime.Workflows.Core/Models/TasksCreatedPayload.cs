using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Models;

public sealed record TasksCreatedPayload(
    WorkflowId WorkflowId,
    string WorkflowType,
    string PhaseName,
    bool IsParallelPhase,
    IReadOnlyList<TaskProjection> Tasks);

public sealed record TaskProjection(Guid TaskGuid, string PhaseName, PrincipalSid AssignedToSid);