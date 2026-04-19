using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Domain.Entities;

public class HistoricalRecord
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string RouteId { get; init; } = string.Empty;
    public string PackagingType { get; init; } = string.Empty;
    public string CarrierId { get; init; } = string.Empty;
    public string Season { get; init; } = string.Empty; // 'Spring'|'Summer'|'Autumn'|'Winter'
    public bool ExcursionOccurred { get; init; }
    public double? RiskScore { get; init; }
    public DateTimeOffset RecordedAt { get; init; }
    public string PayloadJson { get; init; } = string.Empty;
}

public record ReadinessResult(
    string Status, // "Acceptable" | "Risky" | "NotAcceptable"
    string[] Errors,
    string[] Warnings
);
