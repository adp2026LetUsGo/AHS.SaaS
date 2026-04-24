// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Queries/GetXinferReportQuery.cs
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.Application.Ports;

namespace AHS.Cell.Xinfer.Application.Queries;

public sealed class GetXinferReportQuery(IShipmentReadRepository readRepository)
{
    public async Task<XinferReportDto?> ExecuteAsync(Guid shipmentId, Guid tenantId, CancellationToken ct)
    {
        return (XinferReportDto?)await readRepository.GetSealedReportAsync(shipmentId, tenantId, ct).ConfigureAwait(false);
    }
}
