using Microsoft.Extensions.DependencyInjection;
using Workflows.Core.Interfaces;

namespace Workflows.Core
{
    public sealed class ActivityActivator(IServiceProvider provider) : IActivityActivator
    {
        public TActivity Create<TActivity>(IWorkflowContext workflowContext) where TActivity : class, IWorkflowActivity
        {
            return ActivatorUtilities.CreateInstance<TActivity>(provider, workflowContext);
        }
    }
}