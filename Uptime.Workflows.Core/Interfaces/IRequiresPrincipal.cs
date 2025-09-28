using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IRequiresPrincipal
{
    PrincipalSid ExecutorSid { get; }
}