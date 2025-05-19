using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using static Uptime.Client.Constants;

namespace Uptime.Client.Authentication;

public class ConditionalAuthorizationMessageHandler  
    : AuthorizationMessageHandler
{
    public ConditionalAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation, IConfiguration config)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: [config[ConfigurationKeys.WorkflowApiUrl]!],
            scopes: [config[ConfigurationKeys.DefaultScope]!]);
    }
}