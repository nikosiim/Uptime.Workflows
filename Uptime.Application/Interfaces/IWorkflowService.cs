using Uptime.Application.Models.Approval;

namespace Uptime.Application.Interfaces;

public interface IWorkflowService
{
    Task<bool> StartWorkflowAsync<TPayload>(TPayload payload) where TPayload : class;
    Task<bool> CompleteTaskAsync(TaskCompletionPayload payload);
}