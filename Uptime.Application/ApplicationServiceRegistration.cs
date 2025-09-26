using ApprovalWorkflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SigningWorkflow;
using Uptime.Workflows.Application.Behaviors;
using Uptime.Workflows.Application.Commands;
using Uptime.Workflows.Application.Messaging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;
using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Application;

public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddSlimMediator(typeof(StartWorkflowCommand).Assembly);

        // Register ExceptionHandlingBehavior *first* so it is the outermost pipeline and can catch exceptions from all inner behaviors and handlers.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PrincipalResolutionBehavior<,>));

        // Support services
        services.AddScoped<IPrincipalResolver, PrincipalResolver>();

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