﻿using System.Text.Json;
using Uptime.Workflows.Core;

namespace SigningWorkflow;

public class SigningWorkflowContext : WorkflowContext
{
    public UserTaskActivityData? SigningTask { get; set; }
    
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<SigningWorkflowContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}