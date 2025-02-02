using Uptime.Web.Application.Definitions;
using Uptime.Web.Application.Services;

namespace Uptime.Web.Infrastructure;

public static class ServiceExtensions
{
    public static void ConfigureWorkflows(this IServiceCollection services, IConfiguration configuration)
    {
        List<WorkflowDefinition> workflows = configuration.GetSection("WorkflowDefinitions").Get<List<WorkflowDefinition>>() ?? [];
        foreach (WorkflowDefinition workflow in workflows)
        {
            workflow.ResolveDialogType();
        }

        services.AddSingleton(new WorkflowRegistry(workflows));
    }
}