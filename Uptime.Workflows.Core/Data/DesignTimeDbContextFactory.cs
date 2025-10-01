using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Uptime.Workflows.Core.Data;

public sealed class DesignTimeWorkflowDbContextFactory
    : IDesignTimeDbContextFactory<WorkflowDbContext>
{
    public WorkflowDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<WorkflowDbContext>();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<WorkflowDbContext>(optional: true)
            .Build();

        string? connectionString = config.GetConnectionString("UptimeDbConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Missing connection string for 'UptimeDbConnection'.");

        builder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(WorkflowDbContext).Assembly.FullName);
        });

        Console.WriteLine($"[EF DesignTime] Using: {connectionString}");
        return new WorkflowDbContext(builder.Options);
    }
}