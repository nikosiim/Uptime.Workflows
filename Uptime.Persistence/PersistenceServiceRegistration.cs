using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uptime.Application.Interfaces;

namespace Uptime.Persistence;

public static class ConnectionStrings
{
    public const string DefaultConnection = "UptimeDbConnection";
}

public static class PersistenceServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString(ConnectionStrings.DefaultConnection)!;

        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Production")
        {
            string? azureConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__UptimeDbConnection");
            if (!string.IsNullOrEmpty(azureConnectionString))
            {
                connectionString = azureConnectionString;
            }
        }

        services.AddDbContext<WorkflowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDbContextFactory<WorkflowDbContext>(
            options => options.UseSqlServer(connectionString),
            ServiceLifetime.Scoped
        );

        services.AddScoped<IWorkflowDbContextFactory, WorkflowDbContextFactory>();
        services.AddScoped<IWorkflowDbContext>(provider => provider.GetRequiredService<WorkflowDbContext>());
    }
}