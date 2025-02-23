using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public interface IApiService
{
    Task<Result<T>> GetJsonAsync<T>(string url);
}