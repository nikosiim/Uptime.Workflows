using System.Security.Claims;
using Uptime.Workflows.Core.Models;

namespace Uptime.Workflows.Core.Services;

public interface IMembershipResolver
{
    /// Returns true when <paramref name="user"/> is the same principal
    /// OR is contained in the SharePoint/O365 group represented by <paramref name="assignee"/>.
    Task<bool> IsMemberAsync(ClaimsPrincipal user, AssigneeRef assignee);
}