using System.ComponentModel.DataAnnotations;
using Uptime.Workflows.Core.Enums;

namespace Uptime.Workflows.Core.Data;

public class WorkflowPrincipal : BaseEntity
{
    /// <summary>
    /// A stable, unique ID for this principal (Azure AD oid, Windows SID, or fallback Name).
    /// </summary>
    [Required, MaxLength(200)]
    public string ExternalId { get; set; } = null!;

    /// <summary>
    /// A human‐readable name (displayName, UPN, or DOMAIN\username).
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// User | SharePointGroup | AdGroup
    /// </summary>
    public PrincipalType Type { get; set; }

    /// <summary>
    /// Which auth source this came from: "AzureAD" or "Windows".
    /// </summary>
    [Required, MaxLength(50)]
    public string Source { get; set; } = null!;

    /// <summary>
    /// Optional email or UPN if present in the token/claims.
    /// </summary>
    [MaxLength(200)] 
    public string? Email { get; set; }
}