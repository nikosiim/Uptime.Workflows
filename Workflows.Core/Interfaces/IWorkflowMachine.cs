using Workflows.Core.Common;
using Workflows.Core.Models;

namespace Workflows.Core.Interfaces;

public interface IWorkflowMachine
{
    Result<string> GetModificationContext(); 
    Task<Result<Unit>> StartAsync(StartWorkflowPayload payload, CancellationToken ct);
    Task<Result<Unit>> ModifyAsync(ModificationPayload payload, CancellationToken ct);
    Task<Result<Unit>> CancelAsync(CancelWorkflowPayload payload, CancellationToken ct);
    Task<Result<Unit>> Rehydrate(string storageJson, string phase, CancellationToken ct);
}