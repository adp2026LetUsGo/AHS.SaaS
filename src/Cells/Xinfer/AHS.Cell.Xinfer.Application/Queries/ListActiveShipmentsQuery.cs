// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Queries/ListActiveShipmentsQuery.cs
using AHS.Cell.Xinfer.Application.DTOs;
using AHS.Cell.Xinfer.Application.Ports;

namespace AHS.Cell.Xinfer.Application.Queries;

public sealed class ListActiveShipmentsQuery(IShipmentReadRepository readRepository)
{
    public async Task<IReadOnlyList<ShipmentSummaryDto>> ExecuteAsync(Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct)
    {
        var results = await readRepository.ListActiveAsync(tenantId, pageSize, afterId, ct).ConfigureAwait(false);
        return results.Cast<ShipmentSummaryDto>().ToList().AsReadOnly();
    }
}
