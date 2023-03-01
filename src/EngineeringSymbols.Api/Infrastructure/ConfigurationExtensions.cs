using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace EngineeringSymbols.Api.Infrastructure;

public static class ConfigurationExtensions{
    public static TConfig AddKeyVault<TConfig>(this TConfig config)
        where TConfig: IConfiguration, IConfigurationBuilder
    {
        var keyVaultName = config["KeyVaultName"];
        if (string.IsNullOrEmpty(keyVaultName))
        {
            throw new Exception("'KeyVaultName' not set in configuration, see readme");
        }
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var credentials = new DefaultAzureCredential();
        var secretClient = new SecretClient(keyVaultUri, credentials);
        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        return config;
    }

    public static FusekiServerSettings GetFusekiSettings(this IConfiguration config)
    {
        var fusekiServers = config.GetSection("FusekiServers").Get<List<FusekiServerSettings>>();

        if (fusekiServers == null || fusekiServers.Count == 0)
        {
            throw new Exception("'FusekiServers' entry in appsettings.json is empty or missing!");
        }
        
        return fusekiServers[0];
    }
}