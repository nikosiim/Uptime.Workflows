namespace Uptime.Workflows.Core.Interfaces;

internal interface IUserTaskActivity : IWorkflowActivity
{
    IUserTaskActivityData? TaskData { get; set; }
}