// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Persistence/ShipmentRepository.cs
using AHS.Cell.ColdChain.Domain.Aggregates;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace AHS.Cell.ColdChain.Infrastructure.Persistence;

public sealed class ShipmentRepository(ColdChainDbContext dbContext) : IShipmentRepository
{
    public async Task AppendAsync(Guid aggregateId, IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.Id == aggregateId, ct);
        
        if (shipment == null)
        {
            shipment = Shipment.Rehydrate(events);
            dbContext.Shipments.Add(shipment);
        }
        else
        {
            shipment.Rehydrate(events);
        }
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<Shipment> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.Id == aggregateId, ct);
        return shipment ?? throw new KeyNotFoundException($"Shipment {aggregateId} not found.");
    }
}
