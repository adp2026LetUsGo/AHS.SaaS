// src/Cells/Xinfer/AHS.Cell.Xinfer.API/Endpoints/ShipmentEndpoints.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AHS.Cell.Xinfer.Application.Handlers;
using AHS.Cell.Xinfer.Application.Queries;
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Common.Infrastructure.Tenancy;
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.API.Endpoints;

internal static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", async (CreateShipmentRequest req, RegisterShipmentHandler handler, ITenantContext tenant, CancellationToken ct) =>
        {
            // Mapping to command using default actor for demo purposes
            var cmd = new RegisterShipmentCommand(
                req.CargoType, req.InsulationType, req.OriginLocation, req.DestinationLocation, 
                req.PlannedDeparture, tenant.TenantId, Guid.NewGuid(), "System", req.ReasonForChange);
            
            var id = await handler.HandleAsync(cmd, ct).ConfigureAwait(false);
            return Results.Created($"/api/shipments/{id}", new { id });
        });

        endpoints.MapGet("/", async (ListActiveShipmentsQuery query, ITenantContext tenant, CancellationToken ct) =>
        {
            var results = await query.ExecuteAsync(tenant.TenantId, 50, null, ct).ConfigureAwait(false);
            return Results.Ok(results);
        });

        endpoints.MapGet("/{id:guid}", async (Guid id, GetShipmentByIdQuery query, ITenantContext tenant, CancellationToken ct) =>
        {
            var result = await query.ExecuteAsync(id, tenant.TenantId, ct).ConfigureAwait(false);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        endpoints.MapPost("/{id:guid}/seal", async (Guid id, SealShipmentRequest req, SealShipmentHandler handler, ITenantContext tenant, CancellationToken ct) =>
        {
            var cmd = new SealShipmentCommand(
                id, req.FinalStatus, req.QualityDecision, tenant.TenantId, 
                Guid.NewGuid(), "System", req.ReasonForChange);
            
            await handler.HandleAsync(cmd, ct).ConfigureAwait(false);
            return Results.Ok();
        });

        return endpoints;
    }
}
