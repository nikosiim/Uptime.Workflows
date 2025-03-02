using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Uptime.Application.Common;
using Uptime.Application.Interfaces;
using Uptime.Application.Workflows.Approval;
using Uptime.Application.Workflows.Signing;
using Uptime.Domain.Interfaces;

namespace Uptime.Application;

public static class ApplicationServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped<IWorkflowMachine, ApprovalWorkflow>();
        services.AddScoped<IWorkflowMachine, SigningWorkflow>();

        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
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