using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uptime.Domain.Entities;

namespace Uptime.Persistence.Seeding;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasData(
            new Document { Id = 1, Title = "QuickGuide", Description = "First document", CreatedBy = "Emma Carter", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 2, Title = "PlanDraft", Description = "Second document", CreatedBy = "Sophia Patel", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 3, Title = "Notes2025", Description = "Third document", CreatedBy = "Liam Rodriguez", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 4, Title = "TaskList", Description = "Fourth document", CreatedBy = "Isabella Nguyen", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 5, Title = "IdeaLog", Description = "Fifth document", CreatedBy = "Emma Carter", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 6, Title = "MiniReport", Description = "Sixth document", CreatedBy = "Liam Rodriguez", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 7, Title = "FastSummary", Description = "Seventh document", CreatedBy = "User7", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 8, Title = "BriefMemo", Description = "Eighth document", CreatedBy = "Isabella Nguyen", LibraryId = 1, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 9, Title = "Snapshot", Description = "Ninth document", CreatedBy = "Noah Kim", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Document { Id = 10, Title = "OutlineDoc", Description = "Tenth document", CreatedBy = "Sophia Patel", LibraryId = 2, Created = new DateTime(2015, 5, 15, 13, 45, 0) }
        );
    }
}