// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Oracle/OracleResult.cs
using System;

namespace AHS.Cell.ColdChain.Application.Oracle;

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
