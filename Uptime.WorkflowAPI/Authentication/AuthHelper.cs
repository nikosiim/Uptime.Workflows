using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Microsoft.SharePoint.Client;

namespace Uptime.Workflows.Api.Authentication;

/// <summary>
/// Creates a <see cref="ClientContext"/> that authenticates to SharePoint
/// Online with an <b>app-only certificate</b> – no PnP dependency.
/// </summary>
internal static class AuthHelper
{
    /// <param name="siteUrl">
    /// Full site URL, e.g. https://contoso.sharepoint.com/sites/Workflows
    /// </param>
    /// <param name="tenantId">Azure-AD tenant GUID</param>
    /// <param name="clientId">App-registration (client) ID</param>
    /// <param name="certThumbprint">Thumbprint of the upload-ed certificate</param>
    /// <param name="useMachineStore">
    /// true → look in <c>LocalMachine</c> store;  
    /// false → look in <c>CurrentUser</c> store (default).
    /// </param>
    public static ClientContext CreateSpoContext(
        string siteUrl,
        string tenantId,
        string clientId,
        string certThumbprint,
        bool useMachineStore = false)
    {
        // 1) Load certificate
        X509Certificate2 cert = LoadCertificate(certThumbprint, useMachineStore);

        // 2) Acquire a Bearer token for SharePoint
        var credential = new ClientCertificateCredential(tenantId, clientId, cert);
        var resource = $"{new Uri(siteUrl).Scheme}://{new Uri(siteUrl).Host}";
        string token = credential.GetToken(new TokenRequestContext([$"{resource}/.default"])).Token;

        // 3) Build CSOM context and inject the Authorization header
        var ctx = new ClientContext(siteUrl);
        ctx.ExecutingWebRequest += (_, e) =>
        {
            e.WebRequestExecutor.RequestHeaders["Authorization"] = $"Bearer {token}";
        };

        return ctx;
    }
    
    private static X509Certificate2 LoadCertificate(string thumbprint, bool machineStore)
    {
        var store = new X509Store(StoreName.My, machineStore ? StoreLocation.LocalMachine : StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        try
        {
            X509Certificate2Collection certs =
                store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certs.Count == 0)
                throw new InvalidOperationException($"Certificate with thumbprint '{thumbprint}' not found.");

            return certs[0];
        }
        finally
        {
            store.Close();
        }
    }
}