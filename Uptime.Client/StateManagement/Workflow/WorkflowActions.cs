using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Workflows;

namespace Uptime.Client.StateManagement.Workflow;

public record LoadDocumentsAction;
public record LoadDocumentsFailedAction(string ErrorMessage);
public record LoadDocumentsSuccessAction(List<LibraryDocumentResponse> Documents);

public record LoadWorkflowDefinitionsAction;
public record LoadWorkflowDefinitionsFailedAction(string ErrorMessage);
public record LoadWorkflowDefinitionsSuccessAction(List<WorkflowDefinitionResponse> Definitions);

public record ResetWorkflowTemplatesErrorAction;
public record LoadWorkflowTemplatesAction(int LibraryId);
public record LoadWorkflowTemplatesFailedAction(string ErrorMessage);
public record LoadWorkflowTemplatesSuccessAction(List<LibraryWorkflowTemplateResponse> Templates);
public record UpdateWorkflowTemplateAction(int TemplateId, string TemplateName, string WorkflowName, string WorkflowBaseId, string AssociationDataJson);
public record DeleteWorkflowTemplateAction(int TemplateId);

public record CreateWorkflowTemplateAction(string TemplateName, string WorkflowName, string WorkflowBaseId, int LibraryId, string? AssociationDataJson);
public record CreateWorkflowTemplateSuccessAction(int TemplateId);
public record CreateWorkflowTemplateFailedAction(string ErrorMessage);