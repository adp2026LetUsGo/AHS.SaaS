// src/Foundation/AHS.Common/Contracts/ICellEvent.cs
namespace AHS.Common.Contracts;

public interface ICellEvent
{
    Guid           EventId    { get; }
    string         TenantSlug { get; }
    DateTimeOffset OccurredAt { get; }
    string         CellName   { get; }
}
