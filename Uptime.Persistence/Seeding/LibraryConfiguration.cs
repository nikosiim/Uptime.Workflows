using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uptime.Domain.Entities;

namespace Uptime.Persistence.Seeding;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.HasData(
            new Library { Id = 1, Name = "Documents", Created = new DateTime(2015, 5, 15, 13, 45, 0) },
            new Library { Id = 2, Name = "Letters", Created = new DateTime(2015, 5, 15, 13, 45, 0) }
        );
    }
}