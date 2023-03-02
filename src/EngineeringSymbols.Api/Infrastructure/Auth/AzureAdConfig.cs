namespace EngineeringSymbols.Api.Infrastructure.Auth;

public class AzureAdConfig
{
    public string Instance { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
    public Uri AuthorizationUrl => new($"{Instance}/{TenantId}/oauth2/v2.0/authorize");
    public Uri TokenUrl => new($"{Instance}/{TenantId}/oauth2/v2.0/token");
}