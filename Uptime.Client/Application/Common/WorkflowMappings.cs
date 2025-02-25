namespace Uptime.Client.Application.Common;

public record FormsConfiguration
{
    public required string Id { get; init; }
    public required string InitiationPage { get; init; }
    public required Type AssociationDialogType { get; init; }
}