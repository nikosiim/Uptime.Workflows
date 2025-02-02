using Uptime.Application.Enums;

namespace Uptime.Application.Models.Common;

public interface IWorkflowItem
{
    bool IsCompleted { get; }
}

public class ReplicatorState<T> where T : IWorkflowItem
{
    public ReplicatorType Type { get; set; }
    public List<T> Items { get; set; } = [];
}