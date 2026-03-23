// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Ports/IThermalDataSource.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHS.Common.Engines;
using AHS.Common.Domain;

namespace AHS.Cell.ColdChain.Domain.Ports;

public interface IThermalDataSource
{
    IAsyncEnumerable<ThermalDataPoint> StreamAsync(string zoneId, CancellationToken ct);
}
