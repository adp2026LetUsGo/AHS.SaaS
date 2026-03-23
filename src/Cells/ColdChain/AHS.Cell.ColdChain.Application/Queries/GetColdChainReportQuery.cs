// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Queries/GetColdChainReportQuery.cs
using AHS.Cell.ColdChain.Application.DTOs;
using AHS.Cell.ColdChain.Application.Ports;

namespace AHS.Cell.ColdChain.Application.Queries;

public sealed class GetColdChainReportQuery(IShipmentReadRepository readRepository)
{
    public async Task<ColdChainReportDto?> ExecuteAsync(Guid shipmentId, Guid tenantId, CancellationToken ct)
    {
        return (ColdChainReportDto?)await readRepository.GetSealedReportAsync(shipmentId, tenantId, ct);
    }
}
