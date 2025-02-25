using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
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

        services.AddScoped<ApprovalWorkflow>();
        services.AddScoped<SigningWorkflow>();

        services.AddScoped<IWorkflowMachine>(sp => sp.GetRequiredService<ApprovalWorkflow>());
        services.AddScoped<IWorkflowMachine>(sp => sp.GetRequiredService<SigningWorkflow>());

        services.AddScoped<IWorkflowFactory, WorkflowFactory>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddTransient(typeof(IStateMachineFactory<,>), typeof(StatelessStateMachineFactory<,>));

        services.AddSingleton<IWorkflowDefinition, ApprovalWorkflowDefinition>();
        services.AddSingleton<IWorkflowDefinition, SigningWorkflowDefinition>();
    }
}