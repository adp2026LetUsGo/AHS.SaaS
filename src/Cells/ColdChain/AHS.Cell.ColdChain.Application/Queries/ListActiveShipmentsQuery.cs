// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Queries/ListActiveShipmentsQuery.cs
using AHS.Cell.ColdChain.Application.DTOs;
using AHS.Cell.ColdChain.Application.Ports;

namespace AHS.Cell.ColdChain.Application.Queries;

public sealed class ListActiveShipmentsQuery(IShipmentReadRepository readRepository)
{
    public async Task<IReadOnlyList<ShipmentSummaryDto>> ExecuteAsync(Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct)
    {
        var results = await readRepository.ListActiveAsync(tenantId, pageSize, afterId, ct);
        return results.Cast<ShipmentSummaryDto>().ToList().AsReadOnly();
    }
}
