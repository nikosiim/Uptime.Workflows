using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Models;

public sealed class Principal
{
    public required string Sid { get; init; }
    public required PrincipalId Id { get; init; }
    public required string? Name { get; init; }
    public string? Email { get; init; }
}