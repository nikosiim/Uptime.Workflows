using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class OutboundNotification : IEntity
{
    public int Id { get; init; }

    public OutboundEventType EventType { get; set; }

    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    // Optional: set if the event is about a specific task
    public int? WorkflowTaskId { get; set; }
    public WorkflowTask? WorkflowTask { get; set; }
    public Guid? TaskGuid { get; set; }
    [StringLength(128)]
    public string? PhaseId { get; set; } // Optional replicator metadata

    [StringLength(256)]
    public string EndpointPath { get; set; } = null!;
    public int HttpStatusCode { get; set; }
    public OutboundNotificationStatus Status { get; set; }

    [StringLength(512)]
    public string SourceSiteUrl { get; set; } = null!;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string PayloadJson { get; set; } = null!;

    [StringLength(4000)]
    public string? ResponseBody { get; set; }
    [StringLength(1024)]
    public string? LastError { get; set; }
    public int AttemptCount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public Guid UniqueKey { get; set; }
}