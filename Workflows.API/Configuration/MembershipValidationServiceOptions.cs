namespace Uptime.Workflows.Api.Configuration;

public sealed class MembershipValidationServiceOptions
{
    public string  BaseUrl   { get; init; } = null!;
    public string? ApiKey    { get; init; }
    public TimeSpan Timeout  { get; init; } = TimeSpan.FromSeconds(10);
}