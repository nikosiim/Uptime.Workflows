using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Reflection;
using Uptime.Client;
using Uptime.Client.Application.Services;
using Uptime.Client.Authentication;
using static Uptime.Client.Constants;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind(ConfigurationKeys.AADSection, options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration[ConfigurationKeys.DefaultScope]!);
});

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

builder.Services.AddMudServices();
builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(Program).Assembly));

builder.Services.AddHttpClient<IApiService, ApiService>(ApiRoutes.WorkflowApiClient, client =>
    {
        client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.WorkflowApiUrl]!);
    })
    .AddHttpMessageHandler<ConditionalAuthorizationMessageHandler>();

builder.Services.AddScoped<ConditionalAuthorizationMessageHandler>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CancellationTokenSource>();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();