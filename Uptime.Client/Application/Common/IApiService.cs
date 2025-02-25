using Uptime.Client.Application.DTOs;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Common;

public interface IApiService
{
    Task<Result<WorkflowTemplate>> GetWorkflowTemplateAsync(int templateId);
    Task<Result<T>> GetJsonAsync<T>(string url);
    Task<Result<T>> CreateAsync<TRequest, T>(string url, TRequest payload);
    Task<Result<bool>> UpdateAsync<T>(string url, T payload);
    Task<Result<bool>> DeleteAsync(string url);
}