// src/Cells/ColdChain/AHS.Cell.ColdChain.API/Endpoints/OracleEndpoints.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AHS.Cell.ColdChain.Application.Oracle;

namespace AHS.Cell.ColdChain.API.Endpoints;

public static class OracleEndpoints
{
    public static IEndpointRouteBuilder MapOracleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/calculate", async (OracleRequest req, LogisticsOracle oracle, CancellationToken ct) =>
        {
            var result = await oracle.CalculateAsync(req, ct);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
