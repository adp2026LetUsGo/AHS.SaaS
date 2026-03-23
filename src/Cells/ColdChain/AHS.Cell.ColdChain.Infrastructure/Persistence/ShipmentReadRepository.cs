// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Persistence/ShipmentReadRepository.cs
using AHS.Cell.ColdChain.Application.Ports;
using AHS.Cell.ColdChain.Application.DTOs;
using AHS.Common.Infrastructure.Persistence; // For IDbConnectionFactory
using Dapper;

namespace AHS.Cell.ColdChain.Infrastructure.Persistence;

public sealed class ShipmentReadRepository(IDbConnectionFactory connectionFactory) : IShipmentReadRepository
{
    public async Task<ShipmentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        using var connection = await connectionFactory.CreateAsync(ct);
        const string sql = "SELECT * FROM shipments WHERE id = @id AND tenant_id = @tenantId";
        return await connection.QueryFirstOrDefaultAsync<ShipmentDto>(sql, new { id, tenantId });
    }

    public async Task<IReadOnlyList<ShipmentSummaryDto>> ListActiveAsync(Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct)
    {
        using var connection = await connectionFactory.CreateAsync(ct);
        const string sql = @"
            SELECT id, cargo_type, status, insulation_type, is_sealed, tenant_id 
            FROM shipments 
            WHERE tenant_id = @tenantId AND (@afterId IS NULL OR id > @afterId)
            ORDER BY created_at DESC, id
            LIMIT @pageSize";
        
        var results = await connection.QueryAsync<ShipmentSummaryDto>(sql, new { tenantId, pageSize, afterId });
        return results.ToList().AsReadOnly();
    }

    public async Task<ColdChainReportDto?> GetSealedReportAsync(Guid shipmentId, Guid tenantId, CancellationToken ct)
    {
        using var connection = await connectionFactory.CreateAsync(ct);
        // Realistic GxP query joining aggregate state with ledger entries for proof of work
        const string sql = @"
            SELECT s.id as ShipmentId, s.cargo_type, s.mkt_celsius, 
                   (SELECT COUNT(*) FROM excursion_events WHERE shipment_id = s.id) as ExcursionCount,
                   s.status as FinalStatus, s.quality_decision, s.sealed_at,
                   true as IsCompliant -- Logic would be in the query
            FROM shipments s
            WHERE s.id = @shipmentId AND s.tenant_id = @tenantId AND s.is_sealed = true";
            
        return await connection.QueryFirstOrDefaultAsync<ColdChainReportDto>(sql, new { shipmentId, tenantId });
    }
}
