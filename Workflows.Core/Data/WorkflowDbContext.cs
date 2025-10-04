using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Workflows.Core.Data.Seeding;

namespace Workflows.Core.Data;

public class WorkflowDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
    public DbSet<WorkflowHistory> WorkflowHistories { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<WorkflowPrincipal> WorkflowPrincipals { get; set; } = null!;
    public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; } = null!;
    public DbSet<OutboundNotification> OutboundNotifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UptimeAPI");

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<byte[]>("RowVersion")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            }
        }

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
            b.Property(n => n.EventName).IsRequired();
            b.Property(n => n.Status).IsRequired();
            b.Property(n => n.PayloadJson).IsRequired();
            b.Property(n => n.ResponseBody);
            b.Property(n => n.LastError).HasMaxLength(1024);
            b.Property(n => n.CreatedAtUtc).IsRequired();
            b.Property(n => n.AttemptCount).HasDefaultValue(0);
            
            // Helpful indexes
            b.HasIndex(n => n.UniqueKey).IsUnique();
            b.HasIndex(n => new { EventType = n.EventName, n.Status });
            b.HasIndex(n => n.CreatedAtUtc);
        });

        // --- Apply per-entity configurations (if you keep them)
        modelBuilder.ApplyConfiguration(new WorkflowPrincipalConfiguration());
    }
}