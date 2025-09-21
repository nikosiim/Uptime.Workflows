using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Uptime.Workflows.Api.Configuration;

internal static class ServiceExtensions
{
    public static IServiceCollection AddWorkflowAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
        AzureAdSettings ad = cfg.GetSection("AzureAd").Get<AzureAdSettings>()
                             ?? throw new InvalidOperationException("AzureAd configuration section missing.");

        // This "Hybrid" policy scheme will automatically use Bearer if Authorization header is present,
        // otherwise will fall back to Windows Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "HybridAuth";
        })
        .AddPolicyScheme("HybridAuth", "Windows or Bearer", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var hasBearer = context.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true;
                return hasBearer ? JwtBearerDefaults.AuthenticationScheme : "Negotiate";
            };
        })
        // JWT Bearer for Azure-based/cloud callers
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = ad.Authority;
            options.Audience = ad.ApiClientId;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = ad.Authority,
                ValidateAudience = true,
                ValidAudience = ad.ApiClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        })
        // Windows Auth for IIS/on-prem callers
        .AddNegotiate(); // works with IIS Windows Auth

        // Authorization: allow either scheme if user is authenticated,
        // and require correct scope for Bearer tokens (Azure)
        services.AddAuthorization(options =>
        {
            options.AddPolicy("TrustedApp", policy =>
            {
                // This allows either:
                // - a successfully authenticated Windows user, OR
                // - a valid Bearer token with "access_as_admin" scope
                policy.RequireAssertion(context =>
                {
                    // Windows Auth (Negotiate)
                    if (context.User.Identity is { AuthenticationType: "Negotiate", IsAuthenticated: true })
                        return true;

                    // Bearer token
                    var hasScope = context.User.HasClaim(c => c.Type == "scp" && c.Value.Contains("access_as_admin"));
                    return context.User.Identity?.AuthenticationType == JwtBearerDefaults.AuthenticationScheme && hasScope;
                });
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