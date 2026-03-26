// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Oracle/OracleResult.cs
using System;

namespace AHS.Cell.Xinfer.Application.Oracle;

public record OracleResult(
    Guid           RequestId,
    DateTimeOffset CalculatedAt,
    float          RiskScore,
    float          PessimisticTtfHours,
    float          SafeWindowHours,
    XaiDna         Dna,
    string         Recommendation,
    bool           RequiresImmediateAction
);
