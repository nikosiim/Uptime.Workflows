using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Uptime.Workflows.Api.Authentication;
using Uptime.Workflows.Core.Services;
using static Uptime.Workflows.Api.Constants;

namespace Uptime.Workflows.Api.Configuration;

internal static class ServiceExtensions
{
    private static bool IsRunningInAzure(this IHostEnvironment env)
        => env.IsProduction() && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    public static IServiceCollection AddWorkflowAuthentication(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        // ① Bind typed options
        services.Configure<OnPremSharePointOptions>(cfg.GetSection("SharePoint"));
        services.Configure<SpoOnlineOptions>(cfg.GetSection("SharePointOnline"));

        AzureAdSettings ad = cfg.GetSection(ConfigurationKeys.AADSection).Get<AzureAdSettings>()
            ?? throw new InvalidOperationException("Api:AAD section missing.");

        bool inAzure = env.IsRunningInAzure();

        // ② ONE AddAuthentication call – set DefaultScheme immediately
        AuthenticationBuilder auth = services.AddAuthentication(options =>
        {
            options.DefaultScheme = inAzure
                ? AuthSchemes.Bearer          // production = Bearer only
                : AuthSchemes.Combined;       // dev/on-prem  = Negotiate or Bearer
        });

        // ③ Hybrid only: Negotiate + policy scheme
        if (!inAzure)
        {
            auth.AddPolicyScheme(AuthSchemes.Combined, "Negotiate/Bearer", o =>
                {
                    o.ForwardDefaultSelector = ctx =>
                        ctx.Request.Headers.Authorization.FirstOrDefault()
                            ?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                            ? AuthSchemes.Bearer
                            : AuthSchemes.Negotiate;
                })
                .AddNegotiate();
        }

        // ④ Always add JWT Bearer
        auth.AddJwtBearer(AuthSchemes.Bearer, o =>
        {
            o.Authority = ad.Authority;
            o.Audience  = ad.ApiClientId;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType    = "name",
                ValidateIssuer   = true,
                ValidIssuer      = ad.Authority,
                ValidateAudience = true,
                ValidAudience    = ad.ApiClientId
            };
        });

        // ⑤ One resolver registration – choose at runtime
        if (inAzure)
        {
            services.AddSingleton<IMembershipResolver, SpoOnlineMembershipResolver>();
        }
        else
        {
            services.AddSingleton<IMembershipResolver, OnPremMembershipResolver>();
        }

        services.ConfigureOptions<AuthorizationSetup>();
        services.AddHttpClient("sp");
        return services;
    }

    internal static void AddWorkflowSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Workflow API", Version = "v1" });
            c.AddSecurityDefinition(AuthSchemes.Bearer, new OpenApiSecurityScheme
            {
                Name        = HeaderNames.Authorization,
                Scheme      = "bearer",
                Type        = SecuritySchemeType.Http,
                In          = ParameterLocation.Header,
                Description = "Paste JWT here."
            });

            c.OperationFilter<SecureEndpointAuthRequirementFilter>();
        });
    }
}

file sealed class AuthorizationSetup : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(Policies.Admin,
            p => p.RequireClaim("scp", AuthenticationScopes.Cancel));
        options.AddPolicy(Policies.Start,
            p => p.RequireClaim("scp", AuthenticationScopes.Start));
        options.AddPolicy(Policies.Modify,
            p => p.RequireClaim("scp", AuthenticationScopes.Modify));
    }
}