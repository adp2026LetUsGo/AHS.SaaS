// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/RecordExcursionHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Ports;

namespace AHS.Cell.Xinfer.Application.Handlers;

public sealed class RecordExcursionHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(RecordExcursionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct).ConfigureAwait(false);
        
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

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct).ConfigureAwait(false);
    }
}
