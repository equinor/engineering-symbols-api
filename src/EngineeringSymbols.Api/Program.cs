using EngineeringSymbols.Api.Infrastructure;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Api.Repositories.Fuseki;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyVault();

builder.Services.AddRateLimiterFixed(builder.Configuration, builder.Environment);

builder.Services.AddAzureAuthentication(builder.Configuration);

builder.Services.AddRoleBasedAuthorization();
builder.Services.AddFallbackAuthorization();
        
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwaggerGen(builder.Configuration);
        
builder.Services.AddCorsWithPolicyFromAppSettings(builder.Configuration, builder.Environment);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    //options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Services and repos
builder.Services.AddHttpClient<IFusekiService, FusekiService>();
builder.Services.AddTransient<IEngineeringSymbolService, EngineeringSymbolService>();
builder.Services.AddSingleton<IEngineeringSymbolRepository, FusekiRepository>();

// Construct WebApplication
var app = builder.Build();

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseSwaggerAndServeSwaggerApp(builder.Configuration);

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.AddEndpoints();

app.Run();