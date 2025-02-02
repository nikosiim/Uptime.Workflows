namespace Uptime.Application.DTOs;

public record LibraryDocumentDto
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int LibraryId { get; init; }
}