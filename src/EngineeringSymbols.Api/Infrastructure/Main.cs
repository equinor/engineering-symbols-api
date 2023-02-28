using System.Text.Json;
using System.Text.Json.Serialization;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;


namespace EngineeringSymbols.Api.Infrastructure;

public static class Main
{
    public static WebApplication ConstructWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddKeyVault();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamWebApi("fuseki", options =>
            {
                var fusekiServer = builder.Configuration.GetFusekiSettings();
                options.BaseUrl = fusekiServer.BaseUrl;
                options.Scopes = fusekiServer.Scopes;
            })
            .AddInMemoryTokenCaches();
        
        builder.Services.AddFallbackAuthorization();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            //options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        
        
         // Services and repos
         builder.Services.AddHttpClient<IFusekiService, FusekiService>();
        builder.Services.AddTransient<IEngineeringSymbolService, EngineeringSymbolService>();
        builder.Services.AddSingleton<IEngineeringSymbolRepository, FusekiRepository>();
        //builder.Services.AddSingleton<IEngineeringSymbolRepository, InMemoryTestRepository>();
        
        
        
        // Construct WebApplication
        var app = builder.Build();

        app.ServeSwaggerAppInDevelopment();
        
        app.UseHttpsRedirection();
        
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
    

}