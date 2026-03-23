// src/Foundation/AHS.Common/Domain/DomainEvent.cs
namespace AHS.Common.Domain;

public abstract record DomainEvent
{
    public Guid            EventId    { get; init; } = Guid.NewGuid();
    public DateTimeOffset  OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string          EventType  { get; init; } = "";
    public Guid            TenantId   { get; init; }
    public Guid            ActorId    { get; init; }
    public string          ActorName  { get; init; } = "";
    public int             Version    { get; init; } = 1;
}
