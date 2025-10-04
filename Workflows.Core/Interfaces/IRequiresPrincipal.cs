using Workflows.Core.Common;

namespace Workflows.Core.Interfaces;

public interface IRequiresPrincipal
{
    PrincipalSid ExecutorSid { get; }
}