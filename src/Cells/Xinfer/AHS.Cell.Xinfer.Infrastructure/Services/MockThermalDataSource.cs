using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Common.Engines;

namespace AHS.Cell.Xinfer.Infrastructure.Services;

public class MockThermalDataSource : IThermalDataSource
{
    public async IAsyncEnumerable<ThermalDataPoint> StreamAsync(string zoneId, [EnumeratorCancellation] CancellationToken ct)
    {
        // Simple mock streaming 5 data points
        var random = new Random();
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(100, ct);
            yield return new ThermalDataPoint(
                CelsiusValue: 4.0 + (random.NextDouble() * 2 - 1),
                Timestamp: DateTimeOffset.UtcNow,
                ZoneId: zoneId
            );
        }
    }
}
