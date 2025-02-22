using Microsoft.EntityFrameworkCore;
using Uptime.Application.Interfaces;

namespace Uptime.Persistence;

public class WorkflowDbContextFactory(IDbContextFactory<WorkflowDbContext> factory) : IWorkflowDbContextFactory
{
    public IWorkflowDbContext CreateDbContext()
    {
        return factory.CreateDbContext();
    }
}