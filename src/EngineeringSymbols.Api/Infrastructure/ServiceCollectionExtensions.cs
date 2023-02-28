using EngineeringSymbols.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;

namespace EngineeringSymbols.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
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
}