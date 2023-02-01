// using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	//options.RoutePrefix = string.Empty;
});

}

app.UseHttpsRedirection();

var symbols = app.MapGroup("/symbols");

symbols.MapGet("/", GetAllSymbols)
			.WithName("GetSymbolsList")
			.WithOpenApi();


symbols.MapGet("/detailed", GetAllSymbols);
symbols.MapGet("/{id}", GetAllSymbols);
symbols.MapPost("/", GetAllSymbols);
symbols.MapPut("/{id}", GetAllSymbols);
symbols.MapDelete("/{id}", GetAllSymbols);

app.Run();


static async Task<IResult> GetAllSymbols(string db)
{
	return TypedResults.Ok("Yes");
}