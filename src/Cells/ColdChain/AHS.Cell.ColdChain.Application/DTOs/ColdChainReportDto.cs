// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/DTOs/ColdChainReportDto.cs
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Application.DTOs;

public record ColdChainReportDto(
    Guid           ShipmentId,
    CargoType       CargoType,
    double         MktCelsius,
    int            ExcursionCount,
    ShipmentStatus  FinalStatus,
    QualityDecision QualityDecision,
    DateTimeOffset SealedAt,
    bool           IsCompliant
);
