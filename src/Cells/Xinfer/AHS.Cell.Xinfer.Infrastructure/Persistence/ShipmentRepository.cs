// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Persistence/ShipmentRepository.cs
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace AHS.Cell.Xinfer.Infrastructure.Persistence;

public sealed class ShipmentRepository(XinferDbContext dbContext) : IShipmentRepository
{
    public async Task AppendAsync(Guid aggregateId, IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.Id == aggregateId, ct).ConfigureAwait(false);
        
        if (shipment == null)
        {
            shipment = Shipment.Rehydrate(events);
            dbContext.Shipments.Add(shipment);
        }
        else
        {
            ((AggregateRoot)shipment).Rehydrate(events);
        }
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<Shipment> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.Id == aggregateId, ct).ConfigureAwait(false);
        return shipment ?? throw new KeyNotFoundException($"Shipment {aggregateId} not found.");
    }
}
