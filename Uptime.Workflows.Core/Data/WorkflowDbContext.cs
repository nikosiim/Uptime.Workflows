using Microsoft.EntityFrameworkCore;
using Uptime.Workflows.Core.Data.Seeding;

namespace Uptime.Workflows.Core.Data;

public class WorkflowDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Library> Libraries { get; set; } = null!;
    public DbSet<WorkflowHistory> WorkflowHistories { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<WorkflowPrincipal> WorkflowPrincipals { get; set; } = null!;
    public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UptimeAPI");

        // Library -> Documents (Cascade delete)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Library)
            .WithMany(l => l.Documents)
            .HasForeignKey(d => d.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Library -> WorkflowTemplates (Cascade delete)
        modelBuilder.Entity<WorkflowTemplate>()
            .HasOne(wt => wt.Library)
            .WithMany(l => l.WorkflowTemplates)
            .HasForeignKey(wt => wt.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Document -> Workflows (Cascade delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.Document)
            .WithMany(d => d.Workflows)
            .HasForeignKey(w => w.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkflowTemplate -> Workflows (No cascade, restrict delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.WorkflowTemplate)
            .WithMany(wt => wt.Workflows)
            .HasForeignKey(w => w.WorkflowTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workflow -> WorkflowTasks (Cascade delete)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.Workflow)
            .WithMany(w => w.WorkflowTasks)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Workflow -> WorkflowHistories (Cascade delete)
        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.Workflow)
            .WithMany(w => w.WorkflowHistories)
            .HasForeignKey(h => h.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkflowTask -> WorkflowPrincipal (AssignedTo, Restrict delete)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowTask -> WorkflowPrincipal (AssignedBy, Restrict delete)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedByPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workflow -> WorkflowPrincipal (InitiatedBy, Restrict delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.InitiatedByPrincipal)
            .WithMany()
            .HasForeignKey(w => w.InitiatedByPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowHistory -> WorkflowPrincipal (PerformedBy, Restrict delete, nullable FK)
        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.PerformedByPrincipal)
            .WithMany()
            .HasForeignKey(h => h.PerformedByPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowTask model configuration (TaskGuid as external ID)
        modelBuilder.Entity<WorkflowTask>(b =>
        {
            // PK stays Id (clustered by default)
            b.HasKey(t => t.Id);

            // TaskGuid: required, generated on insert if not set, immutable after save
            b.Property(t => t.TaskGuid)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            b.Property(t => t.TaskGuid).Metadata.SetAfterSaveBehavior(
                Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Throw);

            // Alternate key for public lookups
            b.HasIndex(t => t.TaskGuid).IsUnique();

            // Helpful query indexes (tune as needed)
            b.HasIndex(t => new { t.AssignedToPrincipalId, t.InternalStatus });
            b.HasIndex(t => new { t.WorkflowId, t.InternalStatus });
            b.HasIndex(t => t.PhaseId);
        });

        // Apply configurations if any (e.g., for seeding or property configs)
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowPrincipalConfiguration());
    }
}