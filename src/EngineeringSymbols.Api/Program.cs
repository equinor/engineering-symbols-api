// using Microsoft.AspNetCore.OpenApi;

using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.SvgParser.Models;

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

var opts = (SvgParserOptions opt) =>
{
	opt.FillColor = "";
	opt.RemoveAnnotations = true;
	opt.StrokeColor = "sd";
	opt.AnnotationsElementId = "sdsd";
	opt.ConnectorFillColor = "sdsd";
	opt.IncludeSvgString = true;
	opt.IncludeRawSvgString = false;
};