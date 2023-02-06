namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder ServeSwaggerAppInDevelopment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            //options.RoutePrefix = string.Empty;
        });
        
        return app;
    }
}