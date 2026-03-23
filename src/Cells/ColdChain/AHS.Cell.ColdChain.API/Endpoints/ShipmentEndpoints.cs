// src/Cells/ColdChain/AHS.Cell.ColdChain.API/Endpoints/ShipmentEndpoints.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AHS.Cell.ColdChain.Application.Handlers;
using AHS.Cell.ColdChain.Application.Queries;
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Common.Infrastructure.Tenancy;
using AHS.Common.Domain;

namespace AHS.Cell.ColdChain.API.Endpoints;

public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", async (CreateShipmentRequest req, RegisterShipmentHandler handler, ITenantContext tenant, CancellationToken ct) =>
        {
            // Mapping to command using default actor for demo purposes
            var cmd = new RegisterShipmentCommand(
                req.CargoType, req.InsulationType, req.OriginLocation, req.DestinationLocation, 
                req.PlannedDeparture, tenant.TenantId, Guid.NewGuid(), "System", req.ReasonForChange);
            
            var id = await handler.HandleAsync(cmd, ct);
            return Results.Created($"/api/shipments/{id}", new { id });
        });

        endpoints.MapGet("/", async (ListActiveShipmentsQuery query, ITenantContext tenant, CancellationToken ct) =>
        {
            var results = await query.ExecuteAsync(tenant.TenantId, 50, null, ct);
            return Results.Ok(results);
        });

        endpoints.MapGet("/{id:guid}", async (Guid id, GetShipmentByIdQuery query, ITenantContext tenant, CancellationToken ct) =>
        {
            var result = await query.ExecuteAsync(id, tenant.TenantId, ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        endpoints.MapPost("/{id:guid}/seal", async (Guid id, SealShipmentRequest req, SealShipmentHandler handler, ITenantContext tenant, CancellationToken ct) =>
        {
            var cmd = new SealShipmentCommand(
                id, req.FinalStatus, req.QualityDecision, tenant.TenantId, 
                Guid.NewGuid(), "System", req.ReasonForChange);
            
            await handler.HandleAsync(cmd, ct);
            return Results.Ok();
        });

        return endpoints;
    }
}
