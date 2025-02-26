namespace Uptime.Web.Application.DTOs;

public record LibraryDocument
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int LibraryId { get; init; }
}