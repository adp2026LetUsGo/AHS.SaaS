// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Ports/IShipmentRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Domain.Aggregates;

namespace AHS.Cell.ColdChain.Domain.Ports;

public interface IShipmentRepository
{
    Task AppendAsync(Guid aggregateId, IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct);
    Task<Shipment> LoadAsync(Guid aggregateId, CancellationToken ct);
}
