using MudBlazor.Services;
using System.Reflection;
using Uptime.Web;
using Uptime.Web.Infrastructure;
using Uptime.Web.Presentation;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(ApiRoutes.WorkflowApiClient, client =>
{
    client.BaseAddress = new Uri("https://localhost:7250/");
});

builder.Services.AddMudServices();
builder.Services.ConfigureWorkflows(builder.Configuration);

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();