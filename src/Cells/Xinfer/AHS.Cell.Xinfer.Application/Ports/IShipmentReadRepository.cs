// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Ports/IShipmentReadRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Application.Contracts;

namespace AHS.Cell.Xinfer.Application.Ports;

public interface IShipmentReadRepository
{
    Task<ShipmentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ShipmentSummaryDto>> ListActiveAsync(Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct);
    Task<XinferReportDto?> GetSealedReportAsync(Guid shipmentId, Guid tenantId, CancellationToken ct);
}
