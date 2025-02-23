using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Uptime.Client;
using Uptime.Client.Application.Common;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddMudServices();
builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(Program).Assembly));

builder.Services.AddHttpClient<IApiService, ApiService>(ApiRoutes.WorkflowApiClient, client =>
{
    client.BaseAddress = new Uri("https://localhost:7250/");
});

builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<CancellationTokenSource>();

await builder.Build().RunAsync();