
namespace Uptime.Shared.Models.Workflows;

public record ModifyWorkflowRequest
{
    public required string Executor { get; set; }
    public string? ModificationContext { get; set; }
}