namespace Uptime.Client.Application.DTOs;

public static class DocumentSeedData
{
    public static List<LibraryDocument> AllDocuments { get; } =
    [
        new () { Id = 1, Title = "Teabenõue", Description = "Sofia Kuperštein", LibraryId = 1 },
        new() { Id = 2, Title = "LISA_13.01.2025_7-4.2_277-3", Description = "Vello Lauri", LibraryId = 1 },
        new() { Id = 3, Title = "Pöördumine", Description = "SK_25.02.2025_9-11_25_59-4", LibraryId = 2 },
        new() { Id = 4, Title = "LEPING_AS GoTravel_18.12.2024_7-4.2_281", Description = "AS GoTravel", LibraryId = 1 },
        new() { Id = 5, Title = "IdeaLog", Description = "Fifth document", LibraryId = 2 },
        new() { Id = 6, Title = "LEPING_14.02.2025_7-4.2_293", Description = "Rethinkers OÜ", LibraryId = 1 },
        new() { Id = 7, Title = "FastSummary", Description = "Rethinkers OÜ", LibraryId = 2 },
        new() { Id = 8, Title = "2024 inventuuri lõppakt", Description = "PZU Kindlustus", LibraryId = 1 },
        new() { Id = 9, Title = "Intervjuu tervisekassaga", Description = "Riigi IKT Keskus", LibraryId = 2 },
        new() { Id = 10, Title = "Juurdepääsupiirangu muutumine", Description = "Kaitseministeerium", LibraryId = 2 }
    ];
}