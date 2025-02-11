﻿using Uptime.Domain.Enums;

namespace Uptime.Application.Interfaces;

public interface IWorkflowFactory
{
    Task<WorkflowPhase> StartWorkflowAsync(Guid workflowBaseId, IWorkflowPayload payload, CancellationToken cancellationToken);
    IWorkflowMachine? GetWorkflow(Guid workflowBaseId);
}