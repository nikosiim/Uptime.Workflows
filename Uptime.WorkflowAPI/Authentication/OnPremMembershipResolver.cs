using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using System.Security.Claims;
using Uptime.WorkflowAPI.Configuration;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;

namespace Uptime.WorkflowAPI.Authentication;

public sealed class OnPremMembershipResolver(IOptions<OnPremSharePointOptions> opt, ILogger<OnPremMembershipResolver> log)
    : IMembershipResolver
{
    private readonly OnPremSharePointOptions _opt = opt.Value;
    private readonly ILogger<OnPremMembershipResolver> _log = log;

    public async Task<bool> IsMemberAsync(ClaimsPrincipal user, AssigneeRef assignee)
    {
        string login = user.Identity!.Name!; // DOMAIN\user

        using var ctx = new ClientContext(_opt.SiteUrl);
        ctx.Credentials = new NetworkCredential(_opt.UserName, _opt.Password);

        if (assignee.Kind == PrincipalKind.User)
        {
            return login.Equals(assignee.IdOrName, StringComparison.OrdinalIgnoreCase);
        }

        // SharePoint group
        Group spGroup = ctx.Web.SiteGroups.GetByName(assignee.IdOrName);
        UserCollection users = spGroup.Users;
        ctx.Load(users);
        await ctx.ExecuteQueryAsync();

        return users.Any(u => u.LoginName.Equals(login, StringComparison.OrdinalIgnoreCase));
    }
}