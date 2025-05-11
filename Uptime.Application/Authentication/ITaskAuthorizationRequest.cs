using System.Security.Claims;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Application.Authentication;

public interface ITaskAuthorizationRequest
{
    ClaimsPrincipal Caller { get; }
    TaskId TaskId { get; }
}