// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Handlers/RecordExcursionHandler.cs
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Cell.ColdChain.Domain.Ports;

namespace AHS.Cell.ColdChain.Application.Handlers;

public sealed class RecordExcursionHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(RecordExcursionCommand cmd, CancellationToken ct)
    {
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct);
        
        shipment.RecordExcursion(
            cmd.SensorId,
            cmd.ZoneId,
            cmd.ObservedCelsius,
            cmd.MinLimit,
            cmd.MaxLimit,
            cmd.Severity,
            cmd.SignedById,
            cmd.SignedByName,
            cmd.ReasonForChange);

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct);
    }
}
