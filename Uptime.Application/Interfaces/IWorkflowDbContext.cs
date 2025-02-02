using Uptime.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Uptime.Application.Interfaces;

public interface IWorkflowDbContext
{
    DbSet<WorkflowTask> WorkflowTasks { get; }
    DbSet<Document> Documents { get; }
    DbSet<Library> Libraries { get; }
    DbSet<WorkflowHistory> WorkflowHistories { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowTemplate> WorkflowTemplates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}