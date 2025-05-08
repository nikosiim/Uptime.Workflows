namespace Uptime.Workflows.Core.Models;

public enum PrincipalKind { User, Group }

public sealed class AssigneeRef
{
    public PrincipalKind Kind { get; init; }
    public string IdOrName { get; init; } = "";   // e.g. "i:0#.f|membership|john@contoso.com" or group title
}