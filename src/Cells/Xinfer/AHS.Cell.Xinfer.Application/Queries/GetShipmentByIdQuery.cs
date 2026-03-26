// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Queries/GetShipmentByIdQuery.cs
using AHS.Cell.Xinfer.Application.DTOs;
using AHS.Cell.Xinfer.Application.Ports;

namespace AHS.Cell.Xinfer.Application.Queries;

public sealed class GetShipmentByIdQuery(IShipmentReadRepository readRepository)
{
    public async Task<ShipmentDto?> ExecuteAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return (ShipmentDto?)await readRepository.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
    }
}
