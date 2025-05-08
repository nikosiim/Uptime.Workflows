namespace Uptime.Workflows.Core.Models;

public enum PrincipalKind { User, Group }

public sealed class AssigneeRef
{
    public PrincipalKind Type { get; init; }
    public string Id { get; init; } = "";   // e.g. "i:0#.f|membership|john@contoso.com" or group title
    public string? DisplayName { get; init; } // e.g. "John Doe" or "My Group"
}