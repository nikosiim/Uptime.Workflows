using ApprovalWorkflow;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SigningWorkflow;
using System.Reflection;
using Uptime.Workflows.Application.Behaviors;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Application;

public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        // Support services
        services.AddScoped<IPrincipalResolver, PrincipalResolver>();

        // Pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        
        // Core services
        services.AddScoped<IWorkflowMachine, ApprovalWorkflow.ApprovalWorkflow>();
        services.AddSingleton<IWorkflowDefinition, ApprovalWorkflowDefinition>();

        services.AddScoped<IWorkflowMachine, SigningWorkflow.SigningWorkflow>();
        services.AddSingleton<IWorkflowDefinition, SigningWorkflowDefinition>();

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IWorkflowService, WorkflowService>();

        // Workflow factory
        services.AddScoped<IWorkflowFactory>(sp =>
        {
            IEnumerable<IWorkflowMachine> workflows = sp.GetServices<IWorkflowMachine>();
            IEnumerable<IWorkflowDefinition> definitions = sp.GetServices<IWorkflowDefinition>();
            var logger = sp.GetRequiredService<ILogger<WorkflowFactory>>();
            return new WorkflowFactory(workflows, definitions, logger);
        });
    }
}