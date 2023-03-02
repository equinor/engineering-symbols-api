using EngineeringSymbols.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EngineeringSymbols.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("fuseki", options =>
            {
                var fusekiServer = config.GetFusekiSettings();
                options.BaseUrl = fusekiServer.BaseUrl;
                options.Scopes = fusekiServer.Scopes;
            })
            .AddInMemoryTokenCaches();
        
        return services;
    }
    
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Admin, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Roles.Contributor, policy => policy.RequireRole(Roles.Admin, Roles.Contributor));;
        
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
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Engineering Symbols", Version = "v1" });
            
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
            //options.IncludeXmlComments(xmlPath);
            
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
            }] = System.Array.Empty<string>()
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
                             ?? System.Array.Empty<string>();

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
}