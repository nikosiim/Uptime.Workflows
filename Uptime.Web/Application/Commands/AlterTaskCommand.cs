namespace Uptime.Web.Application.Commands;

public record AlterTaskCommand(int TaskId, int WorkflowId, Dictionary<string, object> Storage)
{
}