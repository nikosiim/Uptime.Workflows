using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Uptime.Application.StateMachine;
using Uptime.Application.Workflows.Approval;
using Uptime.Application.Workflows.Signing;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Services;

namespace Uptime.Application;

public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped<IWorkflowMachine, ApprovalWorkflow>();
        services.AddScoped<IWorkflowMachine, SigningWorkflow>();

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddTransient(typeof(IStateMachineFactory<,>), typeof(StatelessStateMachineFactory<,>));

        services.AddSingleton<IWorkflowDefinition, ApprovalWorkflowDefinition>();
        services.AddSingleton<IWorkflowDefinition, SigningWorkflowDefinition>();

        services.AddScoped<IWorkflowFactory>(sp =>
        {
            IEnumerable<IWorkflowMachine> workflows = sp.GetServices<IWorkflowMachine>();
            IEnumerable<IWorkflowDefinition> definitions = sp.GetServices<IWorkflowDefinition>();
            var logger = sp.GetRequiredService<ILogger<WorkflowFactory>>();
            return new WorkflowFactory(workflows, definitions, logger);
        });
    }
}