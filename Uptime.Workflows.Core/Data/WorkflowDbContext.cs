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
    public DbSet<OutboundNotification> OutboundNotifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UptimeAPI");
        
        // --- Library -> Documents (Cascade delete)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Library)
            .WithMany(l => l.Documents)
            .HasForeignKey(d => d.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Library -> WorkflowTemplates (Cascade delete)
        modelBuilder.Entity<WorkflowTemplate>()
            .HasOne(wt => wt.Library)
            .WithMany(l => l.WorkflowTemplates)
            .HasForeignKey(wt => wt.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Document -> Workflows (Cascade delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.Document)
            .WithMany(d => d.Workflows)
            .HasForeignKey(w => w.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- WorkflowTemplate -> Workflows (Restrict delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.WorkflowTemplate)
            .WithMany(wt => wt.Workflows)
            .HasForeignKey(w => w.WorkflowTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Workflow -> WorkflowTasks (Cascade delete)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.Workflow)
            .WithMany(w => w.WorkflowTasks)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Workflow -> WorkflowHistories (Cascade delete)
        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.Workflow)
            .WithMany(w => w.WorkflowHistories)
            .HasForeignKey(h => h.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- WorkflowTask -> WorkflowPrincipal (AssignedTo, Restrict)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- WorkflowTask -> WorkflowPrincipal (AssignedBy, Restrict)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Workflow -> WorkflowPrincipal (InitiatedBy, Restrict)
        modelBuilder.Entity<Workflow>()
            .HasOne(w => w.InitiatedBy)
            .WithMany()
            .HasForeignKey(w => w.InitiatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // --- WorkflowHistory -> WorkflowPrincipal (PerformedBy, Restrict, nullable FK)
        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.PerformedBy)
            .WithMany()
            .HasForeignKey(h => h.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        // --- WorkflowTask model configuration (TaskGuid as external ID)
        modelBuilder.Entity<WorkflowTask>(b =>
        {
            b.Property(t => t.TaskGuid)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            b.Property(t => t.TaskGuid).Metadata.SetAfterSaveBehavior(
                Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Throw);

            b.HasIndex(t => t.TaskGuid).IsUnique();
            b.HasIndex(t => new { t.AssignedToId, t.InternalStatus });
            b.HasIndex(t => new { t.WorkflowId, t.InternalStatus });
            b.HasIndex(t => t.PhaseId);
        });

        // --- OutboundNotification (system/outbox table)
        modelBuilder.Entity<OutboundNotification>(b =>
        {
            b.HasKey(n => n.Id);
            b.Property(n => n.EventType).IsRequired();
            b.Property(n => n.Status).IsRequired();
            b.Property(n => n.EndpointPath).HasMaxLength(256).IsRequired();
            b.Property(n => n.PayloadJson).IsRequired();
            b.Property(n => n.ResponseBody);
            b.Property(n => n.LastError).HasMaxLength(1024);
            b.Property(n => n.CreatedAtUtc).IsRequired();
            b.Property(n => n.AttemptCount).HasDefaultValue(0);

            b.HasOne(n => n.Workflow)
                .WithMany()
                .HasForeignKey(n => n.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(n => n.WorkflowTask)
                .WithMany()
                .HasForeignKey(n => n.WorkflowTaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // Helpful indexes
            b.HasIndex(n => n.UniqueKey).IsUnique();
            b.HasIndex(n => new { n.WorkflowId, n.EventType, n.Status });
            b.HasIndex(n => n.TaskGuid);
            b.HasIndex(n => n.PhaseId);
            b.HasIndex(n => n.CreatedAtUtc);
        });

        // --- Apply per-entity configurations (if you keep them)
        modelBuilder.ApplyConfiguration(new WorkflowPrincipalConfiguration());
    }
}