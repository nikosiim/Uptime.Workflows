namespace Uptime.Workflows.Api.Configuration;

public sealed class SpoOnlineOptions
{
    public string SiteUrl        { get; init; } = null!;
    public string TenantId       { get; init; } = null!;
    public string ClientId       { get; init; } = null!;
    public string CertThumbprint { get; init; } = null!;
}