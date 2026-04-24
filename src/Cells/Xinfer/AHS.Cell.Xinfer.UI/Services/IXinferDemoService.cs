using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.UI.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Quality Rules", "CA1515:Consider making types internal", Justification = "Injected service must be public.")]
public interface IXinferDemoService
{
    Task<IReadOnlyList<CsvShipmentRecord>> GetShipmentsAsync(CancellationToken ct = default);
    Task<PredictiveShieldMetrics>          GetFleetMetricsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>>        GetAuditEventsAsync(CancellationToken ct = default);
}
