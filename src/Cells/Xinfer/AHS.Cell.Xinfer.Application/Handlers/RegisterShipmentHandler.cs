// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/RegisterShipmentHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Common.Domain;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Application.Handlers;

public sealed class RegisterShipmentHandler(
    IShipmentRepository repository,
    ICellEventPublisher publisher)
{
    public async Task<Guid> HandleAsync(RegisterShipmentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        _ = publisher;
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

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, 0, ct).ConfigureAwait(false);
        
        // Publish created event omitted for brevity or handled by infra
        return shipment.Id;
    }
}
// Note: ICellEventPublisher will be used by Infrastructure to dispatch to Service Bus.
