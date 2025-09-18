namespace Uptime.Workflows.Api.Contracts;

public record LibraryDocumentResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int LibraryId { get; init; }
}