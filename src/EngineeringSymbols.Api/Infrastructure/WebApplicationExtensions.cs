using EngineeringSymbols.Api.Infrastructure.Auth;

namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder ServeSwaggerApp(this WebApplication app, IConfiguration config)
    {
        //if (!app.Environment.IsDevelopment()) return app;
        
        var azureAdConfig = config.GetSection("AzureAd").Get<AzureAdConfig>();
        if (azureAdConfig == null) { throw new InvalidOperationException("Missing 'AzureAd' configuration"); }
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            //options.RoutePrefix = string.Empty;
            options.OAuthAppName("Engineering Symbols");
            options.OAuthUsePkce();
            options.OAuthClientId(azureAdConfig.ClientId);
            options.EnablePersistAuthorization();
        });
        
        return app;
    }
}