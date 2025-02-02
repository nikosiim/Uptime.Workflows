using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uptime.Application.Interfaces;

namespace Uptime.Persistence;

public static class ConnectionStrings
{
    public const string DefaultConnection = "WorkflowsDbConnection";
}

public static class PersistenceServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorkflowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(ConnectionStrings.DefaultConnection)));

        services.AddScoped<IWorkflowDbContext>(provider => provider.GetRequiredService<WorkflowDbContext>());
    }
}