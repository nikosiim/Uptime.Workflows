namespace Uptime.Workflows.Core.Interfaces
{
    public interface IActivityActivator
    {
        TActivity Create<TActivity>(IWorkflowContext workflowContext) where TActivity : class, IWorkflowActivity;
    }
}