namespace Uptime.Application.Interfaces;

public interface IWorkflowDbContextFactory
{
    IWorkflowDbContext CreateDbContext();
}