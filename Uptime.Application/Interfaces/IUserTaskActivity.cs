namespace Uptime.Application.Interfaces;

internal interface IUserTaskActivity : IWorkflowActivity
{
    IUserTaskActivityData? TaskData { get; set; }
}