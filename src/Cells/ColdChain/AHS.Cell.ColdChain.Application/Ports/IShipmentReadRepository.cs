// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Ports/IShipmentReadRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.ColdChain.Application.DTOs;

namespace AHS.Cell.ColdChain.Application.Ports;

public interface IShipmentReadRepository
{
    Task<ShipmentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ShipmentSummaryDto>> ListActiveAsync(Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct);
    Task<ColdChainReportDto?> GetSealedReportAsync(Guid shipmentId, Guid tenantId, CancellationToken ct);
}
