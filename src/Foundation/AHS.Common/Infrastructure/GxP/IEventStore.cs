// src/Foundation/AHS.Common/Infrastructure/GxP/IEventStore.cs
using AHS.Common.Domain;

namespace AHS.Common.Infrastructure.GxP;

public interface IEventStore
{
    Task AppendAsync(Guid aggregateId, string aggregateType,
        IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct);
    Task<IReadOnlyList<DomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct);
    Task<IReadOnlyList<DomainEvent>> LoadFromVersionAsync(
        Guid aggregateId, int fromVersion, CancellationToken ct);
}
