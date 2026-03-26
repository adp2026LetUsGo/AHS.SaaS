using AHS.Common.Domain;

namespace AHS.Web.UI.Services;

public interface IXinferDemoService
{
    Task<IReadOnlyList<CsvShipmentRecord>> GetShipmentsAsync(CancellationToken ct = default);
    Task<PredictiveShieldMetrics>          GetFleetMetricsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>>        GetAuditEventsAsync(CancellationToken ct = default);
}
