// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Ports/IShipmentRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Aggregates;

namespace AHS.Cell.Xinfer.Domain.Ports;

public interface IShipmentRepository
{
    Task AppendAsync(Guid aggregateId, IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct);
    Task<Shipment> LoadAsync(Guid aggregateId, CancellationToken ct);
}
