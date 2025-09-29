using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uptime.Workflows.Core.Data;

namespace Uptime.Workflows.Core;

public static class ConnectionStrings
{
    public const string DefaultConnection = "UptimeDbConnection";
    public const string DefaultConnectionDev = "UptimeDbConnection_dev";
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

        // -----------------------------------------------------------------------------
        // Why we ALSO register IDbContextFactory<WorkflowDbContext>
        // -----------------------------------------------------------------------------
        // • A single WorkflowDbContext instance is NOT thread-safe.
        //   If we spin up parallel tasks (Task.Run, Parallel.ForEach, background
        //   workers, etc.) or queue work that outlives the current HTTP request,
        //   sharing the scoped DbContext will trigger “DbContext is already in use”
        //   or change-tracker corruption.
        //
        // • IDbContextFactory<T> solves that by giving us a way to create a BRAND-NEW
        //   DbContext on demand, each with its own change tracker and connection:
        //
        //      await using var db = await _dbCtxFactory.CreateDbContextAsync(cancellationToken);
        //      // safe to use ‘db’ in parallel with other contexts
        //
        // • We register the factory with ServiceLifetime.Scoped
        //   ( *not* Singleton ) so it can depend on other scoped EF services without
        //   hitting “Cannot consume scoped service from singleton” at startup.
        //
        // • Typical usage:
        //   – Controllers / MediatR handlers   → inject WorkflowDbContext (scoped)
        //   – Parallel loops, hosted services  → inject IDbContextFactory<WorkflowDbContext>
        // -----------------------------------------------------------------------------
        services.AddDbContextFactory<WorkflowDbContext>(opt => opt.UseSqlServer(connectionString), ServiceLifetime.Scoped);
    }
}