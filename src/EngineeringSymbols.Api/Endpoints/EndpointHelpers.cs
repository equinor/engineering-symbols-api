using System.Security.Claims;
using EngineeringSymbols.Tools;
using EngineeringSymbols.Tools.Models;
using EngineeringSymbols.Tools.SvgParser;
using EngineeringSymbols.Tools.Validation;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;

namespace EngineeringSymbols.Api.Endpoints;

public static class EndpointHelpers
{
	public static TryAsync<string> GetSymbolCreateContentFromRequest(HttpRequest request)
		=> async () =>
		{
			var allowedContentTypes = new[] { ContentTypes.Json, ContentTypes.Svg };

			if (request.ContentType is null || !allowedContentTypes.Contains(request.ContentType))
			{
				return new Result<string>(new ValidationException(
					$"Unsupported Content-Type. Expected ${string.Join(" or ", allowedContentTypes)}, but got {request.ContentType}"));
			}

			const long maxSizeInKiB = 500;
			var bodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

			if (bodySizeFeature is not null)
			{
				// NOTE: This will throw BadHttpRequestException 413 on stream.ReadToEndAsync() if body
				// size is too large
				bodySizeFeature.MaxRequestBodySize = maxSizeInKiB * 1024;
			}

			if (request.ContentLength == null || request.ContentLength / 1024 > maxSizeInKiB)
			{
				return new Result<string>(new ValidationException(
					$"Content size is 0 or greater than {maxSizeInKiB} KiB"));
			}

			using var stream = new StreamReader(request.Body);

			return await stream.ReadToEndAsync();
		};

	public static TryAsync<EngineeringSymbolCreateDto> ParseSymbolCreateContent(string? contentType, string? content)
		=> async () =>
		{
			if (content is null || contentType is null)
			{
				return new Result<EngineeringSymbolCreateDto>(
					new ValidationException("Failed to deserialize symbol content."));
			}

			Either<Exception, EngineeringSymbolCreateDto> parsedDto =
				Left(new Exception("Failed to deserialize symbol content"));

			switch (contentType)
			{
				case ContentTypes.Json:
					{
						try
						{
							var obj = JsonConvert.DeserializeObject<EngineeringSymbolCreateDto>(content);

							if (obj != null)
							{
								parsedDto = obj;
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							parsedDto = new ValidationException($"Failed to deserialize JSON content.");
						}

						break;
					}
				case ContentTypes.Svg:
					parsedDto = SvgParser.FromString(content)
						.Match<Either<Exception, EngineeringSymbolCreateDto>>(
							Succ: result =>
							{
								if (result.ParseErrors.Count > 0)
									return new ValidationException(
										result.ParseErrors.ToDictionary(pair => pair.Key,
											pair => pair.Value.ToArray()));

								if (result.EngineeringSymbolSvgParsed == null)
									return new ValidationException("SVG parse error");

								return result.EngineeringSymbolSvgParsed.ToCreateDto();
							},
							Fail: exception => exception);
					break;
				default:
					parsedDto = new ValidationException(
						"Failed to deserialize symbol content. Invalid Content-Type");
					break;
			}

			return parsedDto.Match<Result<EngineeringSymbolCreateDto>>(dto => dto,
				ex => new Result<EngineeringSymbolCreateDto>(ex));
		};

	public static TryAsync<string> GetRequestBodyAsString(HttpRequest request)
		=> async () =>
		{
			using var stream = new StreamReader(request.Body);
			var content = await stream.ReadToEndAsync();
			return content;
		};

	public static TryAsync<User> GetUserInformation(ClaimsPrincipal claimsPrincipal)
		=> async () =>
		{
			var claims = claimsPrincipal.Claims.ToList();

			var oidString = claims.Find(claim => claim.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

			if (!Guid.TryParse(oidString, out var oid))
			{
				return new Result<User>(new ValidationException("Failed to determine user id (oid)."));
			}

			var roles = claims.Where(claim => claim.Type == ClaimTypes.Role)
				.Select(claim => claim.Value)
				.ToList();

			var friendlyName = claims.Find(c => c.Type == "preferred_username")?.Value;

			return new User(oid, roles, friendlyName ?? "", "No email yet");
		};


	public static TryAsync<EngineeringSymbolCreateDto> AddUserFromClaimsPrincipal(EngineeringSymbolCreateDto dto, ClaimsPrincipal claimsPrincipal)
		=> async () =>
		{
			
			var claims = claimsPrincipal.Claims.ToList();

			// var oidString = claims.Find(claim => claim.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
			//
			// if (!Guid.TryParse(oidString, out var oid))
			// {
			// 	return new Result<User>(new ValidationException("Failed to determine user id (oid)."));
			// }

			var roles = claims.Where(claim => claim.Type == ClaimTypes.Role)
				.Select(claim => claim.Value)
				.ToList();

			var friendlyName = claims.Find(c => c.Type == "preferred_username")?.Value;


			var creators = dto.Creators;
			creators.Add(new Tools.Entities.User(friendlyName ?? "No name?","No email yet - TODO"));

			return dto with {Creators = creators};
		};
	
	public static TryAsync<EngineeringSymbolCreateDto> ValidateCreateDto(EngineeringSymbolCreateDto dto)
		=> async () =>
		{
			var validationResult = new EngineeringSymbolCreateDtoValidator().Validate(dto);

			return validationResult.IsValid
				? dto
				: new Result<EngineeringSymbolCreateDto>(
					new ValidationException(validationResult.ToDictionary()));
		};

	//http://schemas.microsoft.com/identity/claims/objectidentifier
}