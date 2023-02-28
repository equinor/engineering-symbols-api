using EngineeringSymbols.Api.Repositories;

namespace EngineeringSymbols.Api.Services;

public interface IFusekiService
{
    Task<HttpResponseMessage> QueryAsync(string query, string? accept = null);
    Task<HttpResponseMessage> UpdateAsync(string update, string? accept = null);
}