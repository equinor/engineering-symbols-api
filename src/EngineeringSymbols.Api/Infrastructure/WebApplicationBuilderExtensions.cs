using EngineeringSymbols.Api.Infrastructure.Auth;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomSwagger(this WebApplicationBuilder builder)
    {
        var azureAdConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();
        if (azureAdConfig == null) { throw new InvalidOperationException("Missing 'AzureAd' configuration"); }
        
        builder.Services.AddSwaggerGen(options =>
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
        
        return builder;
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
}