namespace Uptime.Domain.Interfaces;

internal interface IUserTaskActivity : IWorkflowActivity
{
    IUserTaskActivityData? TaskData { get; set; }
}