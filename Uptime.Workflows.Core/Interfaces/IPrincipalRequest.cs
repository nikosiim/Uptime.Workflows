using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IPrincipalRequest
{
    string ExecutedBySid { get; }
    Principal ExecutedBy { get; set; }
}