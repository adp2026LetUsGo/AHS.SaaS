// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/SealShipmentHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Common.Domain;
using AHS.Common.Engines; // For MeanKineticTemperature
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Application.Handlers;

public sealed class SealShipmentHandler(
    IShipmentRepository repository,
    IThermalDataSource thermalSource,
    ICellEventPublisher publisher)
{
    public async Task HandleAsync(SealShipmentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        _ = publisher;
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct).ConfigureAwait(false);
        
        // 1. Gather thermal data for MKT
        var readings = new List<ThermalDataPoint>();
        await foreach (var point in thermalSource.StreamAsync(shipment.Id.ToString(), ct).ConfigureAwait(false))
        {
            readings.Add(point);
        }

        // 2. Calculate MKT (Blueprint requirement)
        var temps = readings.Select(r => r.CelsiusValue).ToArray();
        double mkt = MeanKineticTemperature.Calculate(temps);

        // 3. Seal Aggregate
        shipment.Seal(
            cmd.FinalStatus,
            mkt,
            cmd.QualityDecision,
            cmd.SignedById,
            cmd.SignedByName,
            cmd.ReasonForChange);

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct).ConfigureAwait(false);
        
        // Publish ShipmentSealed to Service Bus would happen here via infra
    }
}
