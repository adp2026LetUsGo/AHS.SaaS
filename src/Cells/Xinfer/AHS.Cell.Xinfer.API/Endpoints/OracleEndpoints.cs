// src/Cells/Xinfer/AHS.Cell.Xinfer.API/Endpoints/OracleEndpoints.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AHS.Cell.Xinfer.Application.Oracle;

namespace AHS.Cell.Xinfer.API.Endpoints;

internal static class OracleEndpoints
{
    public static IEndpointRouteBuilder MapOracleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/calculate", async (OracleRequest req, LogisticsOracle oracle, CancellationToken ct) =>
        {
            var result = await LogisticsOracle.CalculateAsync(req, ct).ConfigureAwait(false);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
