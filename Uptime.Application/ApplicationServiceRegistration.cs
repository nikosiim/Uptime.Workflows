using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Uptime.Application.Interfaces;
using Uptime.Application.Services;

namespace Uptime.Application;
public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
    }
}