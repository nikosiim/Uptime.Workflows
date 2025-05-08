namespace Uptime.WorkflowAPI.Configuration;

public sealed class OnPremSharePointOptions
{
    public string SiteUrl  { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
}