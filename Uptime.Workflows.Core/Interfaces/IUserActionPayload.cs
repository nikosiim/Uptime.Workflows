using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Core.Interfaces;

public interface IUserActionPayload
{
    PrincipalSid ExecutorSid { get; }
}