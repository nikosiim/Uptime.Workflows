using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Uptime.Workflows.Api.Configuration;
using Uptime.Workflows.Core.Models;
using Uptime.Workflows.Core.Services;
using static Uptime.Workflows.Api.Constants;

namespace Uptime.Workflows.Api.Authentication;

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

        if (a.Type is PrincipalKind.User)
            return upn.Equals(a.Id, StringComparison.OrdinalIgnoreCase);

        Group g = ctx.Web.SiteGroups.GetByName(a.Id);
        ctx.Load(g, gg => gg.Users);
        await ctx.ExecuteQueryAsync();

        return g.Users.Any(u => u.Email.Equals(upn, StringComparison.OrdinalIgnoreCase));
    }
}