﻿using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;
using Uptime.Shared.Choices;

namespace Uptime.Application.Workflows.Signing;

public sealed class SigningWorkflowDefinition : IWorkflowDefinition
{
    public Type Type => typeof(SigningWorkflow);
    public string Name => Type.Name;
    public string DisplayName => "Allkirjastamise töövoog";
    public string Id => "52C90EB4-7F3D-4A1D-B469-3F3B064F76D7";

    public WorkflowDefinition GetDefinition()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            Name = Name,
            DisplayName = DisplayName,
            Actions = [ButtonAction.Signing]
        };
    }

    public ReplicatorConfiguration? ReplicatorConfiguration => null;
}