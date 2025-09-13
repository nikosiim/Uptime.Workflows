using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Interfaces;

public interface IUserActionPayload
{
    Principal ExecutedBy { get; }
}