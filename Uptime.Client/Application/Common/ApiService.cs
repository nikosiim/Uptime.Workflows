using System.Net.Http.Json;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Extensions;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Client.Application.Common;

public class ApiService(IHttpClientFactory httpClientFactory, CancellationTokenSource globalCancellationTokenSource) : IApiService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

    #region Business-Specific API Methods

    public async Task<Result<bool>> StartWorkflowAsync(string originator, int documentId, int workflowTemplateId, Dictionary<string, string?> storage)
    {
        var payload = new
        {
            Originator = originator,
            DocumentId = documentId,
            WorkflowTemplateId = workflowTemplateId,
            Storage = storage
        };

        return await PostWithoutResponseAsync(ApiRoutes.Workflows.StartWorkflow, payload);
    }

    public async Task<Result<List<LibraryDocument>>> GetLibraryDocumentsAsync(int libraryId)
    {
        string url = ApiRoutes.Libraries.GetDocuments.Replace("{libraryId}", libraryId.ToString());
        Result<List<LibraryDocumentResponse>> result = await GetJsonAsync<List<LibraryDocumentResponse>>(url);

        if (!result.Succeeded)
        {
            return Result<List<LibraryDocument>>.Failure(result.Error);
        }

        List<LibraryDocument> documents = result.Value?.Select(document => new LibraryDocument
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            LibraryId = libraryId
        }).ToList() ?? [];

        return Result<List<LibraryDocument>>.Success(documents);
    }

    public async Task<Result<WorkflowTemplate>> GetWorkflowTemplateAsync(int templateId)
    {
        string url = ApiRoutes.WorkflowTemplates.GetTemplate.Replace("{templateId}", templateId.ToString());
        Result<WorkflowTemplateResponse> result = await GetJsonAsync<WorkflowTemplateResponse>(url);

        if (!result.Succeeded)
        {
            return Result<WorkflowTemplate>.Failure(result.Error);
        }

        WorkflowTemplateResponse template = result.Value!;
    
        return Result<WorkflowTemplate>.Success(new WorkflowTemplate
        {
            Id = template.Id,
            Name = template.Name,
            AssociationDataJson = template.AssociationDataJson,
            Created = template.Created,
            WorkflowBaseId = template.WorkflowBaseId
        });
    }

    public async Task<Result<List<WorkflowTemplate>>> GetWorkflowTemplatesAsync(int libraryId)
    {
        string url = ApiRoutes.Libraries.GetWorkflowTemplates.Replace("{libraryId}", libraryId.ToString());
        Result<List<LibraryWorkflowTemplateResponse>> result = await GetJsonAsync<List<LibraryWorkflowTemplateResponse>>(url);

        if (!result.Succeeded)
        {
            return Result<List<WorkflowTemplate>>.Failure(result.Error);
        }

        List<WorkflowTemplate> templates = result.Value?.Select(template => new WorkflowTemplate
        {
            Id = template.Id,
            WorkflowBaseId = template.WorkflowBaseId,
            Name = template.Name,
            AssociationDataJson = template.AssociationDataJson,
            Created = template.Created
        }).ToList() ?? [];

        return Result<List<WorkflowTemplate>>.Success(templates);
    }

    public async Task<Result<List<DocumentWorkflow>>> GetDocumentWorkflowsAsync(int documentId)
    {
        string url = ApiRoutes.Documents.GetWorkflows.Replace("{documentId}", documentId.ToString());
        Result<List<DocumentWorkflowsResponse>> result = await GetJsonAsync<List<DocumentWorkflowsResponse>>(url);

        if (!result.Succeeded)
        {
            return Result<List<DocumentWorkflow>>.Failure(result.Error);
        }

        List<DocumentWorkflow> workflows = result.Value?.Select(workflow => new DocumentWorkflow
        {
            Id = workflow.Id,
            TemplateId = workflow.TemplateId,
            WorkflowTemplateName = workflow.WorkflowTemplateName,
            StartDate = workflow.StartDate,
            EndDate = workflow.EndDate,
            Outcome = WorkflowResources.Get(workflow.Outcome),
            IsActive = workflow.IsActive
        }).ToList() ?? [];

        return Result<List<DocumentWorkflow>>.Success(workflows);
    }

    public async Task<Result<bool>> UpdateWorkflowTemplateAsync(
        int templateId, int libraryId, string templateName, string workflowName, string workflowBaseId, string associationDataJson)
    {
        string url = ApiRoutes.WorkflowTemplates.UpdateTemplate.Replace("{templateId}", templateId.ToString());

        var updateRequest = new
        {
            TemplateName = templateName,
            WorkflowName = workflowName,
            WorkflowBaseId = workflowBaseId,
            AssociationDataJson = associationDataJson
        };

        return await UpdateAsync(url, updateRequest);
    }

    public async Task<Result<int>> CreateWorkflowTemplateAsync(
        string templateName, int libraryId, string workflowName, string workflowBaseId, string? associationDataJson)
    {
        const string url = ApiRoutes.WorkflowTemplates.CreateTemplate;

        var createRequest = new
        {
            TemplateName = templateName,
            WorkflowName = workflowName,
            WorkflowBaseId = workflowBaseId,
            LibraryId = libraryId,
            AssociationDataJson = associationDataJson
        };

        Result<CreateWorkflowTemplateResponse> result = await CreateAsync<object, CreateWorkflowTemplateResponse>(url, createRequest);

        return result.Succeeded
            ? Result<int>.Success(result.Value?.Id ?? 0)
            : Result<int>.Failure(result.Error);
    }

    public async Task<Result<bool>> DeleteWorkflowTemplateAsync(int templateId)
    {
        string url = ApiRoutes.WorkflowTemplates.DeleteTemplate.Replace("{templateId}", templateId.ToString());

        return await DeleteAsync(url);
    }
    
    #endregion

    #region Generic API Methods

    public async Task<Result<T>> GetJsonAsync<T>(string url)
    {
        CancellationToken linkedToken = GetLinkedCancellationToken();

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url, linkedToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(linkedToken);
                return Result<T>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            var data = await response.Content.ReadFromJsonAsync<T>(linkedToken);
            return Result<T>.Success(data);
        }
        catch (OperationCanceledException)
        {
            return Result<T>.Cancelled();
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> PostWithoutResponseAsync<T>(string url, T payload)
    {
        CancellationToken linkedToken = GetLinkedCancellationToken();

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, linkedToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(linkedToken);
                return Result<bool>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            return Result<bool>.Cancelled();
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<TResponse>> CreateAsync<TRequest, TResponse>(string url, TRequest payload)
    {
        CancellationToken linkedToken = GetLinkedCancellationToken();

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, linkedToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(linkedToken);
                return Result<TResponse>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(linkedToken);

            if (result is null)
            {
                return Result<TResponse>.Failure("Invalid response received.");
            }

            return Result<TResponse>.Success(result);
        }
        catch (OperationCanceledException)
        {
            return Result<TResponse>.Cancelled();
        }
        catch (HttpRequestException ex)
        {
            return Result<TResponse>.Failure($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAsync<T>(string url, T payload)
    {
        CancellationToken linkedToken = GetLinkedCancellationToken();

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, linkedToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(linkedToken);
                return Result<bool>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            return Result<bool>.Cancelled();
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string url)
    {
        CancellationToken linkedToken = GetLinkedCancellationToken();

        try
        {
            using HttpResponseMessage response = await _httpClient.DeleteAsync(url, linkedToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(linkedToken);
                return Result<bool>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            return Result<bool>.Cancelled();
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure($"Request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Unexpected error: {ex.Message}");
        }
    }
    
    #endregion
    
    private CancellationToken GetLinkedCancellationToken(CancellationToken? requestToken = null)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token, requestToken ?? CancellationToken.None).Token;
    }
}