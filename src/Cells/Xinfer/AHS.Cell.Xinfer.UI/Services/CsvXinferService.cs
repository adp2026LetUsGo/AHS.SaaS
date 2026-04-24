using System.Security.Cryptography;
using System.Text;
using AHS.Common.Domain;
using Microsoft.Extensions.Caching.Memory;
using AHS.Cell.Xinfer.UI.Models;

namespace AHS.Cell.Xinfer.UI.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Quality Rules", "CA1515:Consider making types internal", Justification = "Registered service must be public.")]
public class CsvXinferService : IXinferDemoService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "XinferDemoData";
    private static readonly char[] Delimiters = { '\r', '\n' };

    public CsvXinferService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    private async Task<IReadOnlyList<CsvShipmentRecord>> LoadDataAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<CsvShipmentRecord>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        var csvContent = await _httpClient.GetStringAsync(new Uri("data/pharma/stress_test_40.csv", UriKind.Relative), ct).ConfigureAwait(false);
        
        var lines = csvContent.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
        var records = new List<CsvShipmentRecord>();

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length >= 6)
            {
                var timestamp = parts[0];
                var ambientStr = parts[1];
                var cargoStr = parts[2];
                var routeId = parts[3];
                var carrier = parts[4];
                var insulation = parts[5].ToUpperInvariant();

                _ = double.TryParse(ambientStr, out var ambient);
                _ = double.TryParse(cargoStr, out var cargo);

                var id = $"SHP-{Math.Abs(timestamp.GetHashCode(StringComparison.Ordinal)):X8}";
                var delta = ambient - cargo;
                
                // Synthetic calculations mapped for demo purposes
                var riskPercent = Math.Clamp((delta / 40.0) * 100.0, 0, 100); 
                
                string status = riskPercent > 75 ? "CRITICAL" : (riskPercent > 50 ? "ELEVATED" : "NOMINAL");
                string insight = insulation == "PASSIVE" && riskPercent > 50 ? "THERMAL_EXPOSURE" : "NOMINAL_STABILITY";
                double slope = delta * 0.05;
                double timeToFailure = Math.Max(0, 120 - (riskPercent * 1.2));

                records.Add(new CsvShipmentRecord(
                    Id: id,
                    CarrierName: carrier,
                    Insulation: insulation,
                    RouteId: routeId,
                    RiskPercent: riskPercent,
                    ExternalTemp: ambient,
                    Status: status,
                    Insight: insight,
                    Slope: slope,
                    TimeToFailureMin: timeToFailure
                ));
            }
        }

        _cache.Set(CacheKey, records, TimeSpan.FromMinutes(5));
        return records;
    }

    public async Task<IReadOnlyList<CsvShipmentRecord>> GetShipmentsAsync(CancellationToken ct = default)
    {
        return await LoadDataAsync(ct).ConfigureAwait(false);
    }

    public async Task<PredictiveShieldMetrics> GetFleetMetricsAsync(CancellationToken ct = default)
    {
        var records = await LoadDataAsync(ct).ConfigureAwait(false);
        if (!records.Any())
        {
            return new PredictiveShieldMetrics(0, 0, 0, 0, false, 0);
        }

        var minTtf = (float)records.Min(r => r.TimeToFailureMin);
        var avgSlope = (float)records.Average(r => r.Slope);
        var isCritical = records.Any(r => r.Status == "CRITICAL");
        var criticalCount = records.Count(r => r.Status == "CRITICAL");

        return new PredictiveShieldMetrics(
            CurrentDeltaT: (float)records.Average(r => r.ExternalTemp),
            Slope: avgSlope,
            ProjectedDeltaT30: 0f,
            TimeToFailureMin: minTtf,
            IsCritical: isCritical,
            CriticalCount: criticalCount
        );
    }

    public async Task<IReadOnlyList<AuditEvent>> GetAuditEventsAsync(CancellationToken ct = default)
    {
        var records = await LoadDataAsync(ct).ConfigureAwait(false);
        var events = new List<AuditEvent>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < Math.Min(records.Count, 20); i++)
        {
            var r = records[i];
            var timestamp = now.AddSeconds(-i * 30);
            
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(r.Id + r.Status + timestamp.Ticks));
            var hash = Convert.ToHexString(hashBytes)[..12];

            var severity = r.Status == "CRITICAL" ? EventSeverity.Critical : 
                           (r.Status == "ELEVATED" ? EventSeverity.Warning : EventSeverity.Info);

            events.Add(new AuditEvent(
                Timestamp: timestamp,
                EventName: $"RISK_ASSESSMENT_{r.Status}",
                ActionTaken: $"ORACLE_LENS_SCANNED_{r.Id}",
                DigitalSignature: hash,
                Severity: severity
            ));
        }

        return events;
    }
}
