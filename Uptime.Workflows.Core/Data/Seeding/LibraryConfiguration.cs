using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uptime.Workflows.Core.Entities;

namespace Uptime.Workflows.Core.Data.Seeding;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.HasData(
            new Library { Id = 1, Name = "Lepingud", Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Library { Id = 2, Name = "Kirjavahetus", Created = new DateTime(2015, 5, 15, 13, 45, 0) }
        );
    }
}