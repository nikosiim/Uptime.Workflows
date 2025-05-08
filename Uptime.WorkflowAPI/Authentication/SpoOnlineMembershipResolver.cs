using Azure.Core;
using Azure.Identity;
using System.Security.Claims;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.Options;
using Uptime.WorkflowAPI.Configuration;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;
using static Uptime.WorkflowAPI.Constants;

namespace Uptime.WorkflowAPI.Authentication;

public sealed class SpoOnlineMembershipResolver(IOptions<SpoOnlineOptions> opt, ILogger<SpoOnlineMembershipResolver> log)
    : IMembershipResolver
{
    private readonly SpoOnlineOptions _opt = opt.Value;
    private readonly ILogger<SpoOnlineMembershipResolver> _log = log;

    public async Task<bool> IsMemberAsync(ClaimsPrincipal user, AssigneeRef a)
    {
        string upn = user.FindFirst(ClaimNames.Upn)?.Value
                     ?? user.FindFirst(ClaimNames.PreferredUserName)?.Value
                     ?? throw new InvalidOperationException("UPN claim missing");

        using ClientContext ctx = AuthHelper.CreateSpoContext(
            _opt.SiteUrl, _opt.TenantId, _opt.ClientId, _opt.CertThumbprint);

        if (a.Kind is PrincipalKind.User)
            return upn.Equals(a.IdOrName, StringComparison.OrdinalIgnoreCase);

        Group g = ctx.Web.SiteGroups.GetByName(a.IdOrName);
        ctx.Load(g, gg => gg.Users);
        await ctx.ExecuteQueryAsync();

        return g.Users.Any(u => u.Email.Equals(upn, StringComparison.OrdinalIgnoreCase));
    }
}