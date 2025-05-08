namespace Uptime.Workflows.Core.Services;

public interface IMembershipResolver
{
    /// <summary>
    /// True if <paramref name="candidate"/> is the person identified by
    /// <paramref name="target"/> **or** is currently registered as
    /// the target’s substitute (vacation / absence replacement).
    /// </summary>
    Task<bool> IsSameUserOrSubstituteAsync(
        string candidate,   // DOMAIN\alice  or  alice@contoso.com
        string target,      // DOMAIN\bob    or  bob@contoso.com
        CancellationToken ct = default);

    /// <summary>
    /// True when <paramref name="user"/> belongs to the
    /// SharePoint or AD group <paramref name="groupName"/>.
    /// </summary>
    Task<bool> IsUserInGroupAsync(
        string user,
        string groupName,   // “Approvers”, “DOMAIN\WF_Admins”
        CancellationToken ct = default);
}