using System.Security.Claims;

namespace EngineeringSymbols.Api.Infrastructure.Auth;

public static class AuthHelpers
{
    public static bool IsContributorOrAdmin(this ClaimsPrincipal user)
    {
        var userIsContributorOrAdmin =
            ServiceCollectionExtensions.ContributorOrAdminRoles.Any(
                role => user.HasClaim(ClaimTypes.Role, role));

        return user.Identity?.IsAuthenticated == true && userIsContributorOrAdmin;
    }
}