using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public record SetCurrentUserAction(string UserName);

public record LoadWorkflowDefinitionsAction;
public record LoadWorkflowDefinitionsFailedAction(string ErrorMessage);
public record LoadWorkflowDefinitionsSuccessAction(List<WorkflowDefinitionResponse> Response);

public record LoadWorkflowTasksAction(int WorkflowId);
public record LoadWorkflowTasksFailedAction(string ErrorMessage);
public record LoadWorkflowTasksSuccessAction(List<WorkflowTasksResponse> Response, int WorkflowId);

public record LoadWorkflowHistoryAction(int WorkflowId);
public record LoadWorkflowHistoryFailedAction(string ErrorMessage);
public record LoadWorkflowHistorySuccessAction(List<WorkflowHistoryResponse> Response);

public record LoadWorkflowDetailsAction(int WorkflowId);
public record LoadWorkflowDetailsFailedAction(string ErrorMessage);
public record LoadWorkflowDetailsSuccessAction(WorkflowDetailsResponse Response, int WorkflowId);