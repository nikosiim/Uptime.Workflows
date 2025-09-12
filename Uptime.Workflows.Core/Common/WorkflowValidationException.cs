namespace Uptime.Workflows.Core.Common;

public sealed class WorkflowValidationException(ErrorCode error, string message) : Exception(message)
{
    public ErrorCode Error { get; } = error;
}