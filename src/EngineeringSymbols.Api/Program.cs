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

//symbols.MapGet("/", GetAllSymbols)
//			.WithName("GetSymbolsList")
//			.WithOpenApi();


//symbols.MapGet("/detailed", GetAllSymbols);
//symbols.MapGet("/{id}", GetAllSymbols);
symbols.MapPost("/", Upload);
//symbols.MapPut("/{id}", GetAllSymbols);
//symbols.MapDelete("/{id}", GetAllSymbols);

app.Run();


static async Task<IResult> Upload(IFormFile file)
{
	var length = file.Length;
	if (length <= 0)
		return TypedResults.Problem("No");

	await using var fileStream = file.OpenReadStream();
	var bytes = new byte[length];
	var a = fileStream.Read(bytes, 0, (int)file.Length);

	var svgString = System.Text.Encoding.UTF8.GetString(bytes);

	return SvgParser.FromString(svgString, opt =>
		{ opt.IncludeSvgString = false; })
		.Match<IResult>(result =>
		{
			if (result.IsSuccess && result.EngineeringSymbol != null)
			{
				return TypedResults.Ok(result.EngineeringSymbol);
			}

			return TypedResults.ValidationProblem(
				result.ParseErrors.Fold(
					new Dictionary<string, string[]>(), (map, pair) =>
					{
						map.Add(pair.Key, pair.Value.ToArray());
						return map;
					}));
		},
		failure =>
		{
			if (failure is SvgParseErrorException exception)
			{
				return TypedResults.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
			}

			return TypedResults.Problem(failure.Message, statusCode: StatusCodes.Status500InternalServerError);
		});
}