// src/Foundation/AHS.Common/Infrastructure/GxP/LedgerEntry.cs
namespace AHS.Common.Infrastructure.GxP;

public record LedgerEntry
{
    public long           Sequence      { get; init; }
    public Guid           TenantId      { get; init; }
    public Guid           AggregateId   { get; init; }
    public string         AggregateType { get; init; } = "";
    public string         EventType     { get; init; } = "";
    public string         PayloadJson   { get; init; } = "";
    public string         ActorId       { get; init; } = "";
    public string         ActorName     { get; init; } = "";
    public DateTimeOffset OccurredAt    { get; init; }
    public string         PreviousHash  { get; init; } = "GENESIS";
    public string         EntryHash     { get; init; } = "";
    public string         HmacSeal      { get; init; } = "";
}
