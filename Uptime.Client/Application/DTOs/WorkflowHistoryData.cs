using Uptime.Client.Application.Common;
using Uptime.Client.Contracts;

namespace Uptime.Client.Application.DTOs;

public class WorkflowHistoryData
{
    public int Id { get; set; }
    public string? User { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset Occurred { get; set; }
    public string? Description { get; set; }
    public int WorkflowId { get; set; }
    public WorkflowEventType Event { get; set; }
    public string EventText => Event.GetTranslation();
}