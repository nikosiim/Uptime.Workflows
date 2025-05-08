using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.SharePoint.Client;
using Uptime.Workflows.Core.Services;
using Group = Microsoft.SharePoint.Client.Group;

namespace Uptime.WorkflowAPI.Authentication;

public sealed class GraphMembershipResolver : IMembershipResolver
{
    private readonly string _siteUrl;
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _certThumbprint;
    private readonly GraphServiceClient _graph;
    private readonly ILogger<GraphMembershipResolver> _log;

    public GraphMembershipResolver(IConfiguration cfg, ILogger<GraphMembershipResolver> log)
    {
        IConfigurationSection s = cfg.GetSection("SharePointOnline");
        _siteUrl = s["SiteUrl"] ?? throw new ArgumentNullException("SiteUrl");
        _tenantId = s["TenantId"] ?? throw new ArgumentNullException("TenantId");
        _clientId = s["ClientId"] ?? throw new ArgumentNullException("ClientId");
        _certThumbprint = s["CertThumbprint"] ?? throw new ArgumentNullException("CertThumbprint");
        _log = log;

        var cred = new ClientCertificateCredential(_tenantId, _clientId, _certThumbprint);
        _graph = new GraphServiceClient(cred, ["https://graph.microsoft.com/.default"]);
    }

    public async Task<bool> IsUserInGroupAsync(string username, string groupName)
    {
        // 1) Try SharePoint site group first
        using ClientContext ctx = AuthHelper.CreateSpoContext(_siteUrl, _tenantId, _clientId, _certThumbprint);
        GroupCollection? siteGroups = ctx.Web.SiteGroups;
        ctx.Load(siteGroups, g => g.Include(sg => sg.Title, sg => sg.Users));
        await ctx.ExecuteQueryAsync();

        Group? spGroup = siteGroups.FirstOrDefault(g => g.Title.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        if (spGroup != null)
        {
            return spGroup.Users.Any(u => 
                u.LoginName.Equals(username, StringComparison.OrdinalIgnoreCase) || 
                u.Email.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // 2) Fall back to Azure AD group
        string? userId  = await GetUserIdAsync(username);
        string? groupId = await GetGroupIdAsync(groupName);
        if (userId == null || groupId == null) 
            return false;

        return await IsMemberOfAadGroupAsync(userId, groupId);
    }

    public async Task<IEnumerable<string>> GetUserGroupsAsync(string username)
    {
        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // SharePoint groups
        using ClientContext ctx = AuthHelper.CreateSpoContext(_siteUrl, _tenantId, _clientId, _certThumbprint);

        GroupCollection? siteGroups = ctx.Web.SiteGroups;
        ctx.Load(siteGroups, g => g.Include(sg => sg.Title, sg => sg.Users));
        await ctx.ExecuteQueryAsync();

        foreach (Group? g in siteGroups.Where(g => g.Users.Any(u =>
                     u.LoginName.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                     u.Email.Equals(username, StringComparison.OrdinalIgnoreCase))))
        {
            groups.Add(g.Title);
        }

        // AAD groups
        string? userOid = await GetUserIdAsync(username);
        if (userOid == null)
            return groups;

        DirectoryObjectCollectionResponse? aadGroupTitles = await _graph
            .Users[userOid].TransitiveMemberOf.GetAsync(r
                => r.QueryParameters.Select = ["displayName"]);

        if (aadGroupTitles?.Value != null)
        {
            foreach (Microsoft.Graph.Models.Group g in aadGroupTitles.Value.OfType<Microsoft.Graph.Models.Group>())
            {
                if (g.DisplayName != null)
                {
                    groups.Add(g.DisplayName);
                }
            }
        }

        return groups;
    }
    
    private async Task<string?> GetUserIdAsync(string username)
    {
        UserCollectionResponse? res = await _graph.Users.GetAsync(r =>
        {
            r.QueryParameters.Filter = $"userPrincipalName eq '{username}'"; 
            r.QueryParameters.Select = ["id"];
        });

        return res?.Value?.FirstOrDefault()?.Id;
    }

    private async Task<string?> GetGroupIdAsync(string displayName)
    {
        GroupCollectionResponse? res = await _graph.Groups.GetAsync(r =>
        {
            r.QueryParameters.Filter = $"displayName eq '{displayName.Replace("'", "''")}'";
            r.QueryParameters.Select = ["id"];
        });

        return res?.Value?.FirstOrDefault()?.Id;
    }

    private async Task<bool> IsMemberOfAadGroupAsync(string userOid, string groupOid)
    {
        DirectoryObjectCollectionResponse? page = await _graph
            .Groups[groupOid].TransitiveMembers.GetAsync(r => 
                r.QueryParameters.Select = ["id"]);

        if (page?.Value?.Any(m => m.Id == userOid) == true) 
            return true;

        while (!string.IsNullOrEmpty(page?.OdataNextLink))
        {
            page = await _graph.Groups[groupOid].TransitiveMembers.WithUrl(page.OdataNextLink).GetAsync();
            if (page?.Value?.Any(m => m.Id == userOid) == true) 
                return true;
        }

        return false;
    }
}
