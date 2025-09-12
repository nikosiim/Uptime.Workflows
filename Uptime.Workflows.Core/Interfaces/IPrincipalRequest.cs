using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IPrincipalRequest // TODO: remove this interfaceand related implementations, principal is resolved in workflow engine
{
    string CallerSid { get; }
    Principal? Caller { get; set; }
}