using Uptime.Client.Application.Common;

namespace Uptime.Client.Application.Services;

public interface IApiService
{
    Task<Result<string?>> ReadAsRawStringAsync(string url, CancellationToken? token = null);
    Task<Result<T>> ReadFromJsonAsync<T>(string url, CancellationToken? token = null);
    Task<Result<T>> CreateAsync<TRequest, T>(string url, TRequest payload, CancellationToken token);
    Task<Result<bool>> PostAsJsonAsync<T>(string url, T payload, CancellationToken token);
    Task<Result<bool>> UpdateAsync<T>(string url, T payload, CancellationToken token);
    Task<Result<bool>> DeleteAsync(string url, CancellationToken token);
}