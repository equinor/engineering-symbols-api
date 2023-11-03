using EngineeringSymbols.Api.Infrastructure;
using EngineeringSymbols.Api.Repositories;
using EngineeringSymbols.Api.Repositories.Fuseki;
using EngineeringSymbols.Tools.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogging();

builder.Configuration.AddKeyVault();

builder.Services.AddRateLimiterFixed(builder.Configuration, builder.Environment);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddRoleBasedAuthorization();
builder.Services.AddFallbackAuthorization();
        
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwaggerGen(builder.Configuration);
        
builder.Services.AddCorsWithPolicyFromAppSettings(builder.Configuration, builder.Environment);

builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    //options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Load the static symbol frame from json
//await SymbolFrames.Load(builder.Configuration.GetValue<string>("SymbolFramePath"));
await SymbolFrames.Load();

// Services and repos
builder.Services.AddHttpClient<IFusekiService, FusekiService>();
builder.Services.AddScoped<IEngineeringSymbolService, EngineeringSymbolService>();
builder.Services.AddScoped<IEngineeringSymbolRepository, FusekiRepository>();

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