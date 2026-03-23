// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Handlers/RegisterShipmentHandler.cs
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Cell.ColdChain.Domain.Aggregates;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Common.Domain;
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Application.Handlers;

public sealed class RegisterShipmentHandler(
    IShipmentRepository repository,
    ICellEventPublisher publisher)
{
    public async Task<Guid> HandleAsync(RegisterShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = Shipment.Create(
            cmd.CargoType,
            cmd.InsulationType,
            cmd.OriginLocation,
            cmd.DestinationLocation,
            cmd.PlannedDeparture,
            cmd.TenantId,
            cmd.SignedById,
            cmd.SignedByName,
            cmd.ReasonForChange);

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, 0, ct);
        
        // Publish created event omitted for brevity or handled by infra
        return shipment.Id;
    }
}
// Note: ICellEventPublisher will be used by Infrastructure to dispatch to Service Bus.
