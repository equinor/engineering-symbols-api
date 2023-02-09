using System.Text.Json;
using System.Text.Json.Serialization;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Api.Repositories.Fuseki;

namespace EngineeringSymbols.Api.Infrastructure;

public static class Main
{
    public static WebApplication ConstructWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddHttpClient<IFusekiService, FusekiService>();
        
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            //options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        
        builder.Services.AddTransient<IEngineeringSymbolService, EngineeringSymbolService>();

        
        builder.Services.AddSingleton<IEngineeringSymbolRepository, FusekiRepository>();
        //builder.Services.AddSingleton<IEngineeringSymbolRepository, InMemoryTestRepository>();
        
        
        
        // Construct WebApplication
        var app = builder.Build();

        app.ServeSwaggerAppInDevelopment();
        
        app.UseHttpsRedirection();
        
        //app.UseAuthentication();
        //app.UseAuthorization();
        return app;
    }
    

}