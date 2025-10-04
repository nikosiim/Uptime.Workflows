using Workflows.Core.Common;

namespace Workflows.Core.Interfaces;

public interface IUserActionPayload
{
    PrincipalSid ExecutorSid { get; }
}