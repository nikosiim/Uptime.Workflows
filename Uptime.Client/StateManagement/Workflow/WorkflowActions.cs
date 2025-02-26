using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public record LoadWorkflowDefinitionsAction;
public record LoadWorkflowDefinitionsFailedAction(string ErrorMessage);
public record LoadWorkflowDefinitionsSuccessAction(List<WorkflowDefinitionResponse> Definitions);