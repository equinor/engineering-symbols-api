using Microsoft.AspNetCore.Authorization;

namespace EngineeringSymbols.Api.Infrastructure.Auth;

public class FallbackSafeguardHandler : AuthorizationHandler<FallbackSafeguardRequirement>
{

    /// <summary>
    /// Safeguard handler which trigges when we have not explicitly added
    /// [Authorize] or [AllowAnonymous].
    /// Always denies access.
    /// </summary>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FallbackSafeguardRequirement req)
    {
        return Task.CompletedTask;
    }
}