using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class OutboundNotification : BaseEntity
{ 
    public OutboundEventType EventType { get; set; }
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;

    // Optional: set if the event is about a specific task
    public int? WorkflowTaskId { get; set; }
    public WorkflowTask? WorkflowTask { get; set; }
    public Guid? TaskGuid { get; set; }
    
    [StringLength(32)]
    public string? PhaseId { get; set; } // Optional replicator metadata
    public string EndpointPath { get; set; } = null!;

    // 200..599 if call happened; 0 if not attempted
    public int HttpStatusCode { get; set; }
    public OutboundNotificationStatus Status { get; set; }
    public string PayloadJson { get; set; } = null!;
    public string? ResponseBody { get; set; }
    public string? LastError { get; set; }

    // Delivery control/observability
    public int AttemptCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }

    // Idempotency/dedup (EventType + WorkflowId [+ TaskGuid])
    public string? UniqueKey { get; set; }
}