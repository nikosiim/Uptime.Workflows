using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public sealed class WorkflowPrincipal : IEntity
{
    public int Id { get; init; }

    /// <summary>
    /// A stable, unique ID for this principal (Azure AD oid, Windows SID)
    /// </summary>
    [Required, MaxLength(256)]
    public string ExternalId { get; set; } = null!;
    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;
    [MaxLength(64)]
    public string? LoginName { get; set; }
    [Required, MaxLength(64)]
    public string Source { get; set; } = "SharePoint";
    [MaxLength(128)] 
    public string? Email { get; set; }
    public PrincipalType Type { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? SyncedAtUtc { get; set; }
    public DateTimeOffset? DeactivatedAtUtc { get; set; }
}