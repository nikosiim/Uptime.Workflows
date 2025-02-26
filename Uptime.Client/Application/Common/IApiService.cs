using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public interface IApiService
{
    Task<Result<List<LibraryDocument>>> GetLibraryDocumentsAsync(int libraryId);
    Task<Result<WorkflowTemplate>> GetWorkflowTemplateAsync(int templateId);
    Task<Result<List<WorkflowTemplate>>> GetWorkflowTemplatesAsync(int libraryId);
    Task<Result<List<DocumentWorkflow>>> GetDocumentWorkflowsAsync(int documentId);
    Task<Result<int>> CreateWorkflowTemplateAsync(string templateName, int libraryId, string workflowName, string workflowBaseId, string? associationDataJson);
    Task<Result<bool>> UpdateWorkflowTemplateAsync(int templateId, int libraryId, string templateName, string workflowName, string workflowBaseId, string associationDataJson);
    Task<Result<bool>> DeleteWorkflowTemplateAsync(int templateId);
    Task<Result<bool>> StartWorkflowAsync(string originator, int documentId, int workflowTemplateId, Dictionary<string, string?> storage);
    Task<Result<T>> GetJsonAsync<T>(string url);
    Task<Result<T>> CreateAsync<TRequest, T>(string url, TRequest payload);
    Task<Result<bool>> UpdateAsync<T>(string url, T payload);
    Task<Result<bool>> DeleteAsync(string url);
}