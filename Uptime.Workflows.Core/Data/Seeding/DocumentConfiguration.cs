using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uptime.Workflows.Core.Entities;

namespace Uptime.Workflows.Core.Data.Seeding;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasData(
            new Document { Id = 1, Title = "Teabenõue", Description = "Sofia Kuperštein", CreatedBy = "Lauri Saar", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 2, Title = "LISA_13.01.2025_7-4.2_277-3", Description = "Vello Lauri", CreatedBy = "Riin Koppel", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 3, Title = "Pöördumine", Description = "SK_25.02.2025_9-11_25_59-4", CreatedBy = "Jana Pärn", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 4, Title = "LEPING_AS GoTravel_18.12.2024_7-4.2_281", Description = "AS GoTravel", CreatedBy = "Markus Lepik", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 5, Title = "IdeaLog", Description = "Fifth document", CreatedBy = "Emma Carter", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 6, Title = "LEPING_14.02.2025_7-4.2_293", Description = "Rethinkers OÜ", CreatedBy = "Marta Laine", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 7, Title = "FastSummary", Description = "Rethinkers OÜ", CreatedBy = "Klient Kaks", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 8, Title = "2024 inventuuri lõppakt", Description = "PZU Kindlustus", CreatedBy = "Viljar Laine", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 9, Title = "Intervjuu tervisekassaga", Description = "Riigi IKT Keskus", CreatedBy = "Signe Kask", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 10, Title = "Juurdepääsupiirangu muutumine", Description = "Kaitseministeerium", CreatedBy = "Anton Rebane", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) }
        );
    }
}