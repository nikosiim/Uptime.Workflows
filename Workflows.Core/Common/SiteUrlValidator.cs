using System.Net;

namespace Workflows.Core.Common;

public static class SiteUrlValidator
{
    public static string Normalize(string input)
    {
        string raw = input?.Trim() ?? string.Empty;
        string trimmed = raw.TrimEnd('/');

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            throw new WorkflowValidationException(ErrorCode.Validation, $"Invalid SiteUrl: '{input}'.");

        UriBuilder ub = new(uri) { Host = uri.Host.ToLowerInvariant() };
        return ub.Uri.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Fail fast if the hostname cannot be resolved from the current environment.
    /// </summary>
    public static async Task EnsureHostResolvesAsync(string absoluteUrl, CancellationToken ct)
    {
        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out Uri? uri))
            throw new WorkflowValidationException(ErrorCode.Validation, $"Invalid SiteUrl: '{absoluteUrl}'.");

        try
        {
            // Throws SocketException if name cannot be resolved.
            _ = await Dns.GetHostEntryAsync(uri.Host, ct);
        }
        catch (Exception)
        {
            throw new WorkflowValidationException(ErrorCode.Validation, $"SiteUrl host '{uri.Host}' is not resolvable from the server environment.");
        }
    }
}