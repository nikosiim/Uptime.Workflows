using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uptime.Workflows.Core.Data;

namespace Uptime.Domain;

public static class ConnectionStrings
{
    public const string DefaultConnection = "UptimeDbConnection";
}

public static class CoreServiceRegistration
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
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
        
        services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(connectionString));
    }
}