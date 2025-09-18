using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IPrincipalRequest
{
    string ExecutorSid { get; }
    Principal ExecutedBy { get; set; }
}