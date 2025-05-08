using System.Security.Claims;

namespace Uptime.Workflows.Core.Common;

public static class PrincipalExtensions
{
    public static string? ToUpn(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Upn)?.Value 
        ?? user.FindFirst("preferred_username")?.Value
        ?? user.Identity?.Name; // DOMAIN\login on-prem

    public static string? ToSid(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Sid)?.Value;

    public static string? ToObjectId(this ClaimsPrincipal user) =>
        user.FindFirst("oid")?.Value;
}