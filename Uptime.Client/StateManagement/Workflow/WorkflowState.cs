﻿using Fluxor;
using Uptime.Client.Application.DTOs;
using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;

namespace Uptime.Client.StateManagement.Workflow;

public record WorkflowState
{
    public required QueryState<Result<List<WorkflowDefinition>>> WorkflowDefinitionsQuery { get; init; }

    /// <summary>
    /// This property holds the task data for a specific workflow. 
    /// When switching between workflow task pages, this property is overwritten with new workflow tasks.
    /// 
    /// ⚠ Important Note:
    /// Unlike other queries, we **cannot** use QueryStatus.Uninitialized here to determine if the data is loaded.
    /// If Uninitialized were used, it could result in displaying incorrect tasks from a previously viewed workflow.
    /// Instead, we rely on QueryStatus.Loading and QueryStatus.Loaded to manage state transitions properly.
    /// 
    /// 🔹 Justification for Using Fluxor:
    /// - The state management remains justified since workflow task data needs to be loaded **outside** the workflow tasks page.
    /// - This ensures tasks are available even if they are needed elsewhere in the application.
    /// </summary>
    public required QueryState<Result<List<WorkflowTaskData>>> WorkflowTasksQuery { get; init; }
}

public class WorkflowFeature : Feature<WorkflowState>
{
    public override string GetName() => "Workflow";

    protected override WorkflowState GetInitialState() => new()
    {
        WorkflowDefinitionsQuery = new QueryState<Result<List<WorkflowDefinition>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        },
        WorkflowTasksQuery = new QueryState<Result<List<WorkflowTaskData>>>
        {
            Result = default,
            Status = QueryStatus.Uninitialized
        }
    };
}