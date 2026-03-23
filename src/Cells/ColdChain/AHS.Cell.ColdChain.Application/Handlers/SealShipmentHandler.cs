// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Handlers/SealShipmentHandler.cs
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Common.Domain;
using AHS.Common.Engines; // For MeanKineticTemperature
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Application.Handlers;

public sealed class SealShipmentHandler(
    IShipmentRepository repository,
    IThermalDataSource thermalSource,
    ICellEventPublisher publisher)
{
    public async Task HandleAsync(SealShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct);
        
        // 1. Gather thermal data for MKT
        var readings = new List<ThermalDataPoint>();
        await foreach (var point in thermalSource.StreamAsync(shipment.Id.ToString(), ct))
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

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct);
        
        // Publish ShipmentSealed to Service Bus would happen here via infra
    }
}
