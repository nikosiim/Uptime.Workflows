using Microsoft.Extensions.DependencyInjection;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core
{
    public sealed class ActivityActivator(IServiceProvider provider) : IActivityActivator
    {
        public TActivity Create<TActivity>(IWorkflowContext workflowContext) where TActivity : class, IWorkflowActivity
        {
            return ActivatorUtilities.CreateInstance<TActivity>(provider, workflowContext);
        }
    }
}