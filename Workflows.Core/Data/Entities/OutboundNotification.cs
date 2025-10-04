using System.ComponentModel.DataAnnotations;
using Workflows.Core.Enums;

namespace Workflows.Core.Data;

public sealed class OutboundNotification : IEntity
{
    public int Id { get; init; }
    public Guid UniqueKey { get; set; }
    [StringLength(64)]
    public string EventName { get; set; } = null!;
    public int WorkflowId { get; init; }
    [StringLength(4000)]
    public string PayloadJson { get; set; } = null!;
    public int HttpStatusCode { get; set; }
    [StringLength(4000)]
    public string? ResponseBody { get; set; }
    [StringLength(1024)]
    public string? LastError { get; set; }
    public OutboundNotificationStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}