using EngineeringSymbols.Api.Services;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.SvgParser.Models;

namespace EngineeringSymbols.Api.Infrastructure;

public static class Main
{
    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddTransient<IEngineeringSymbolService, EngineeringSymbolService>();
        
        // Construct WebApplication
        var app = builder.Build();

        app.ServeSwaggerAppInDevelopment();
        
        app.UseHttpsRedirection();
        
        //app.UseAuthentication();
        //app.UseAuthorization();
        return app;
    }
    

}