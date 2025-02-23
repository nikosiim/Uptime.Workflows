using System.Net.Http.Json;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public class ApiService(IHttpClientFactory httpClientFactory, CancellationTokenSource globalCancellationTokenSource) : IApiService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(ApiRoutes.WorkflowApiClient);

    private CancellationToken GetLinkedCancellationToken(CancellationToken? requestToken = null)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token, requestToken ?? CancellationToken.None).Token;
    }

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
}
