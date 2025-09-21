using Uptime.Workflows.Api.Configuration;
using Uptime.Workflows.Application;
using Uptime.Workflows.Core;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWorkflowAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddWorkflowSwagger();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7142",
                "https://uptimeworkflowsweb-ddcab3gybvcbg6a8.northeurope-01.azurewebsites.net",
                "https://uptimeworkflows.azurewebsites.net"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow API v1"));

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

await app.RunAsync();