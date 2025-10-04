using Workflows.Core.Common;

namespace Workflows.Core.Models;

public sealed class Principal
{
    public required PrincipalId Id { get; init; }
    public required PrincipalSid Sid { get; init; }
    public required string? Name { get; init; }
    public string? Email { get; init; }

    /// <summary>
    /// Built-in system account, used for background operations and
    /// workflow engine–initiated actions.
    /// </summary>
    public static Principal System => new()
    {
        Id = new PrincipalId(1), // fixed database ID for system account
        Sid = new PrincipalSid("SYSTEM"),
        Name = "System",
        Email = null
    };

    public static PrincipalSid SystemSid => System.Sid;
}