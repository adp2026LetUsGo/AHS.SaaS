// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Queries/GetShipmentByIdQuery.cs
using AHS.Cell.ColdChain.Application.DTOs;
using AHS.Cell.ColdChain.Application.Ports;

namespace AHS.Cell.ColdChain.Application.Queries;

public sealed class GetShipmentByIdQuery(IShipmentReadRepository readRepository)
{
    public async Task<ShipmentDto?> ExecuteAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return (ShipmentDto?)await readRepository.GetByIdAsync(id, tenantId, ct);
    }
}
