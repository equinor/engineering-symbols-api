using EngineeringSymbols.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;

namespace EngineeringSymbols.Api.Infrastructure;

public static class WebApplicationExtensions
{
	public static IApplicationBuilder UseSwaggerAndServeSwaggerApp(this WebApplication app, IConfiguration config)
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

		// Deal with exceptions that could occur before entering the TryAsync monad
		app.UseExceptionHandler(configure =>
		{
			configure.Run(async context =>
			{
				var exceptionHandlerPathFeature =
					context.Features.Get<IExceptionHandlerPathFeature>();

				var error = exceptionHandlerPathFeature?.Error;

				context.Response.StatusCode = error switch
				{
					BadHttpRequestException => StatusCodes.Status400BadRequest,
					_ => StatusCodes.Status500InternalServerError,
				};

				if (context.RequestServices.GetService<IProblemDetailsService>() is IProblemDetailsService problemDetailsService)
				{
					await problemDetailsService.WriteAsync(new ProblemDetailsContext
					{
						HttpContext = context,
						ProblemDetails = {
							Detail = error?.Message ?? "",
						}
					});
				}
				else
				{
					context.Response.ContentType = MediaTypeNames.Text.Plain;
					await context.Response.WriteAsync(error?.Message ?? "");
				}
			});
		});

		return app;
	}
}