using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;
using Uptime.Domain.Entities;
using Uptime.Persistence.Seeding;

namespace Uptime.Persistence;

public class WorkflowDbContext(DbContextOptions options) : DbContext(options), IWorkflowDbContext
{
    public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Library> Libraries { get; set; } = null!;
    public DbSet<WorkflowHistory> WorkflowHistories { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
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

        // Document -> WorkflowInstances (Cascade delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(wi => wi.Document)
            .WithMany(d => d.Workflows)
            .HasForeignKey(wi => wi.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkflowTemplate -> WorkflowInstances (No cascade delete)
        modelBuilder.Entity<Workflow>()
            .HasOne(wi => wi.WorkflowTemplate)
            .WithMany(wt => wt.Workflows)
            .HasForeignKey(wi => wi.WorkflowTemplateId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid cycles

        // WorkflowInstance -> WorkflowTasks (Cascade delete)
        modelBuilder.Entity<WorkflowTask>()
            .HasOne(wt => wt.Workflow)
            .WithMany(wi => wi.WorkflowTasks)
            .HasForeignKey(wt => wt.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade); // Tasks should be deleted when a workflow instance is deleted

        // Apply configurations
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryConfiguration());
    }
}