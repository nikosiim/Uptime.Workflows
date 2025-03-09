using Uptime.Application;
using Uptime.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Workflow API", Version = "v1" });
});

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
app.UseRouting();
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();