using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IWorkflowMachine
{
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> StartAsync(StartWorkflowPayload payload, CancellationToken ct);
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken ct);
    Task<Result<Unit>> CancelAsync(CancelWorkflowPayload payload, CancellationToken ct);
    Task<Result<Unit>> Rehydrate(string storageJson, string phase, CancellationToken ct);
}