using System.Threading.RateLimiting;
using EngineeringSymbols.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Array = System.Array;

namespace EngineeringSymbols.Api.Infrastructure;

public static class ServiceCollectionExtensions
{

    public static readonly string[] ContributorOrAdminRoles = { Role.Admin, Role.Contributor };
    
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policy.OnlyAdmins, policy => policy.RequireRole(Role.Admin))
            .AddPolicy(Policy.ContributorOrAdmin, policy => policy.RequireRole(ContributorOrAdminRoles)); ;

        return services;
    }

    public static IServiceCollection AddFallbackAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Setup safeguard if not Authorize or AllowAnonymous is present on endpoint
            var fallbackPolicyBuilder = new AuthorizationPolicyBuilder();
            fallbackPolicyBuilder.Requirements.Add(new FallbackSafeguardRequirement());
            options.FallbackPolicy = fallbackPolicyBuilder.Build();
        });

        services.AddSingleton<IAuthorizationHandler, FallbackSafeguardHandler>();
        return services;
    }

    public static IServiceCollection AddCustomSwaggerGen(this IServiceCollection services, IConfiguration config)
    {
        var azureAdConfig = config.GetSection("AzureAd").Get<AzureAdConfig>();
        if (azureAdConfig == null) { throw new InvalidOperationException("Missing 'AzureAd' configuration"); }

        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Engineering Symbols", Version = "v1" });

            options.AddSecurityDefinition(SecuritySchemeType.OAuth2.ToString(), new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "Engineering Symbols OpenId Security Scheme",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = azureAdConfig.AuthorizationUrl,
                        TokenUrl = azureAdConfig.TokenUrl,
                        Scopes = new Dictionary<string, string>
                        {
                            {$"api://{azureAdConfig.ClientId}/user_impersonation", "Sign in on your behalf"}
                        }
                    }
                }
            });

            options.AddAuthRequirementToAllSwaggerEndpoints();
        });

        return services;
    }

    private static void AddAuthRequirementToAllSwaggerEndpoints(this SwaggerGenOptions options)
    {
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = SecuritySchemeType.OAuth2.ToString()
                }
            }] = Array.Empty<string>()
        });
    }


    public static IServiceCollection AddCorsWithPolicyFromAppSettings(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {

        if (env.IsDevelopment())
        {
            services.AddCors(options =>
                options.AddDefaultPolicy(policyBuilder =>
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                    ));

            return services;
        }

        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? Array.Empty<string>();

        if (!allowedOrigins.Any())
        {
            Console.WriteLine("Warning: No CORS configured!");
        }

        services.AddCors(options =>
            options.AddDefaultPolicy(policyBuilder =>
                policyBuilder
                    .WithOrigins(allowedOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                ));

        return services;
    }

    public static IServiceCollection AddRateLimiterFixed(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0
        // PermitLimit to 4 and the time Window to 12. A maximum of 4 requests per each 12-second window are allowed.
        services.AddRateLimiter(rlOptions => rlOptions
            .AddFixedWindowLimiter(policyName: RateLimiterPolicy.Fixed, options =>
            {
                options.PermitLimit = 500;
                options.Window = TimeSpan.FromSeconds(12);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }));

        return services;
    }
}