using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Services;
using Uptime.Application.Workflows.Approval;

namespace Uptime.Application;
public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped<ApprovalWorkflow>();
        services.AddScoped(typeof(ReplicatorManager<>));
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IWorkflowActivityFactory<ApprovalTaskData>, ApprovalWorkflowActivityFactory>();
    }
}