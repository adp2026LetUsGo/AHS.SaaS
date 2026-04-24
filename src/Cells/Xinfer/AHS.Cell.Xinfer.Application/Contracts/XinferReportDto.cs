// src/Cells/Xinfer/AHS.Cell.Xinfer.Application.Contracts/XinferReportDto.cs
using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Application.Contracts;

public record XinferReportDto(
    Guid           ShipmentId,
    CargoType       CargoType,
    double         MktCelsius,
    int            ExcursionCount,
    ShipmentStatus  FinalStatus,
    QualityDecision QualityDecision,
    DateTimeOffset SealedAt,
    bool           IsCompliant
);
