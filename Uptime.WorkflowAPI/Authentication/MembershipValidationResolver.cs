using Uptime.Workflows.Core.Services;

namespace Uptime.Workflows.Api.Authentication;

public sealed class MembershipValidationResolver(IHttpClientFactory clientFactory, ILogger<MembershipValidationResolver> log)
    : IMembershipResolver
{
    private readonly HttpClient _client = clientFactory.CreateClient("MembershipValidationServiceClient");
    private readonly ILogger<MembershipValidationResolver> _log = log;

    public async Task<bool> IsSameUserOrSubstituteAsync(string candidate, string target, CancellationToken ct = default)
    {
        if (candidate.Equals(target, StringComparison.OrdinalIgnoreCase))
            return true;

        var url = $"substitute?candidate={Uri.EscapeDataString(candidate)}&target={Uri.EscapeDataString(target)}";

        var resp = await _client.GetFromJsonAsync<BoolDto>(url, ct);
        return resp?.Value == true;
    }

    public async Task<bool> IsUserInGroupAsync(string user, string group, CancellationToken ct = default)
    {
        var url = $"groupmember?user={Uri.EscapeDataString(user)}&group={Uri.EscapeDataString(group)}";

        var resp = await _client.GetFromJsonAsync<BoolDto>(url, ct);
        return resp?.Value == true;
    }

    private sealed record BoolDto(bool Value);
}