namespace EngineeringSymbols.Api.Infrastructure.Auth;

public static class Role
{
    public const string Admin = "Symbols.FullAccess";
    public const string Contributor = "Symbols.Contributor";
    public const string God = "Symbols.God";
}

public static class Policy
{
    public const string OnlyAdmins = "OnlyAdmins";
    public const string ContributorOrAdmin = "ContributorOrAdmin";
    public const string God = "ContributorOrAdmin";
}