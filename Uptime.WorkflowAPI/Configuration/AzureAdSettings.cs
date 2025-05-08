namespace Uptime.WorkflowAPI.Configuration;

internal record AzureAdSettings
{
    public string Instance { get; init; } = null!;
    public string? Domain { get; init; }
    public string ApiClientId { get; init; } = null!;
    public string Authority => $"{Instance}{Domain}/v2.0/";
}