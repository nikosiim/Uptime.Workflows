using System.Net.Http.Json;
using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.Client.Application.Common;

public class ApiService(IHttpClientFactory httpClientFactory, CancellationTokenSource globalCancellationTokenSource) : IApiService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

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

    #region Generic methods

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