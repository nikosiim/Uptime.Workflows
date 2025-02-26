using System.Net.Http.Json;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public class ApiService(IHttpClientFactory httpClientFactory) : IApiService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);
   
    public async Task<Result<T>> ReadFromJsonAsync<T>(string url, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url, token);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(token);
                return Result<T>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            var data = await response.Content.ReadFromJsonAsync<T>(token);
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

    public async Task<Result<bool>> PostAsJsonAsync<T>(string url, T payload, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, token);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(token);
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

    public async Task<Result<TResponse>> CreateAsync<TRequest, TResponse>(string url, TRequest payload, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, token);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(token);
                return Result<TResponse>.Failure($"Error {response.StatusCode}: {errorMessage}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(token);

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

    public async Task<Result<bool>> UpdateAsync<T>(string url, T payload, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, token);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(token);
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

    public async Task<Result<bool>> DeleteAsync(string url, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.DeleteAsync(url, token);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync(token);
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
}