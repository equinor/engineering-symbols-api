using System.Net;
using EngineeringSymbols.Api.Repositories.Fuseki;
using Microsoft.AspNetCore.Mvc;

namespace EngineeringSymbols.Api.Endpoints;

public static class DevFuseki
{
    public static async Task<IResult> Query(IFusekiService fuseki, HttpRequest request)
    {
        var query = await GetRequestBodyAsString(request);
        var queryResponse = await fuseki.QueryAsync(query);

        if (queryResponse.StatusCode != HttpStatusCode.OK)
        {
            var msg = await queryResponse.Content.ReadAsStringAsync();
            return TypedResults.BadRequest(msg);
        }
        
        var result = await queryResponse.Content.ReadFromJsonAsync<FusekiSelectResponse>();

        return TypedResults.Ok(result);
    }
    
    public static async Task<IResult> Update(IFusekiService fuseki, HttpRequest request)
    {
        var updateQuery = await GetRequestBodyAsString(request);
        var updateResponse = await fuseki.UpdateAsync(updateQuery);
        var result = await updateResponse.Content.ReadAsStringAsync();
        return TypedResults.Ok(result);
    }

    private static async Task<string> GetRequestBodyAsString(HttpRequest request)
    {
        string content;
        using (var stream = new StreamReader(request.Body))
        {
            content = await stream.ReadToEndAsync();
        }

        if (content == string.Empty)
            throw new Exception("Empty body");

        return content;
    }
}

