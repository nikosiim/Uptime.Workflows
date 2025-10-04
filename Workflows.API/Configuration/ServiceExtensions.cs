using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Workflows.Api.Configuration;

internal static class ServiceExtensions
{
    public static IServiceCollection AddAzureWorkflowAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
        AzureAdSettings ad = cfg.GetSection("Api:AAD").Get<AzureAdSettings>()
                             ?? throw new InvalidOperationException("Api:AAD configuration section missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = ad.Instance + ad.Domain + "/v2.0";
                options.Audience = ad.ApiClientId;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TrustedApp", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    string? scopes =
                        context.User.FindFirst("scp")?.Value ??
                        context.User.FindFirst("http://schemas.microsoft.com/identity/claims/scope")?.Value;
                    
                    return scopes != null && scopes.Contains("access_as_admin");
                });
            });
        });

        return services;
    }

    public static IServiceCollection AddIisWorkflowAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddAuthentication("Negotiate")
            .AddNegotiate();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TrustedApp", policy =>
            {
                policy.RequireAssertion(context => context.User.Identity is { AuthenticationType: "Negotiate", IsAuthenticated: true });
            });
        });

        return services;
    }

    internal static void AddWorkflowSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Workflow API", 
                Version = "v1",
                Description = @"This API requires a client credentials token from the registered SharePoint Gateway app.
                              End user authentication is handled by the gateway.
                              Do not use end user tokens directly against this API."
            });
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name        = HeaderNames.Authorization,
                Scheme      = "bearer",
                Type        = SecuritySchemeType.Http,
                In          = ParameterLocation.Header,
                Description = "Paste JWT here. The token must be a client credentials token from the SharePoint Gateway app. Do not use end user tokens."
            });

            c.OperationFilter<SecureEndpointAuthRequirementFilter>();
        });
    }
}

public static class AuthDebug
{
    public static ILogger? Logger { get; set; }
}