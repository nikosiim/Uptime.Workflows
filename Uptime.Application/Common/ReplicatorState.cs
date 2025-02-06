using Uptime.Application.Enums;
using Uptime.Application.Interfaces;

namespace Uptime.Application.Common;

public class ReplicatorState<T> where T : IReplicatorItem
{
    public ReplicatorType Type { get; set; }
    public List<T> Items { get; set; } = [];
}