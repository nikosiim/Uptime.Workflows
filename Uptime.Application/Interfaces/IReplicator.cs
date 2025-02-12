﻿using Uptime.Application.Common;
using Uptime.Application.Enums;
using Uptime.Domain.Common;

namespace Uptime.Application.Interfaces;

public interface IReplicator<TItem>
{
    ReplicatorType Type { get; set; }
    List<ReplicatorItem<TItem>> Items { get; set; }
    Task ExecuteAsync();
    bool IsComplete { get; }
}