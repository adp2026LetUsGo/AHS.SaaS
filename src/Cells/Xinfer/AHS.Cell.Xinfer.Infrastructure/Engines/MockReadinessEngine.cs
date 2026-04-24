// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Engines/MockReadinessEngine.cs
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace AHS.Cell.Xinfer.Infrastructure.Engines;

public sealed class MockReadinessEngine : IDataReadinessEngine
{
    public ReadinessState Evaluate(ShipmentIdentity identity, CarrierProfile carrier)
    {
        // Simulation Logic adapted to Domain Types
        
        if (identity.RouteId.StartsWith("SHORT-", StringComparison.OrdinalIgnoreCase))
        {
            return new NotAcceptableState(
                Issues: ["Insufficient historical data (n < 5)"],
                Recommendations: ["Expand sample size for this route/carrier combination."]
            );
        }

        if (identity.RouteId.StartsWith("RISKY-", StringComparison.OrdinalIgnoreCase))
        {
            return new RiskyState("Sample size is marginal (n=7). Confidence may be lower than nominal.");
        }

        return new AcceptableState();
    }
}
