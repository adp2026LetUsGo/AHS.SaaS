// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Ports/IDataReadinessEngine.cs
using AHS.Cell.Xinfer.Domain.ValueObjects;
using System.Collections.Generic;

namespace AHS.Cell.Xinfer.Domain.Ports;

public abstract record ReadinessState;

public sealed record NotAcceptableState(
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Recommendations
) : ReadinessState;

public sealed record RiskyState(
    string Explanation
) : ReadinessState;

public sealed record AcceptableState() : ReadinessState;

public interface IDataReadinessEngine
{
    // Refactored for Domain Sovereignty: uses Domain Value Objects instead of API DTOs.
    ReadinessState Evaluate(ShipmentIdentity identity, CarrierProfile carrier);
}
