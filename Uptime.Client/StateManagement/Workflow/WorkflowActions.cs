using Uptime.Client.Application.DTOs;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public record LoadWorkflowDefinitionsAction;
public record LoadWorkflowDefinitionsFailedAction(string ErrorMessage);
public record LoadWorkflowDefinitionsSuccessAction(List<WorkflowDefinitionResponse> Response);

public record LoadWorkflowTasksAction(int WorkflowId);
public record LoadWorkflowTasksFailedAction(string ErrorMessage);
public record LoadWorkflowTasksSuccessAction(List<WorkflowTasksResponse> Response, int WorkflowId);