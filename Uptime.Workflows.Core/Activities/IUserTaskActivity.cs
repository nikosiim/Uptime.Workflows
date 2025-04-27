namespace Uptime.Workflows.Core;

internal interface IUserTaskActivity : IWorkflowActivity
{
    IUserTaskActivityData? TaskData { get; set; }
}