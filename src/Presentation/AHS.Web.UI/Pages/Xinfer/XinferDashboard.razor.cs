using System.Diagnostics;
using AHS.Common.Domain;
using AHS.Web.UI.Services;
using Microsoft.AspNetCore.Components;

namespace AHS.Web.UI.Pages.Xinfer;

public partial class XinferDashboard : ComponentBase, IDisposable
{
    [Inject] private IXinferDemoService DataService { get; set; } = default!;

    private PredictiveShieldMetrics _fleetMetrics = new(0, 0, 0, 0, false, 0);
    private IReadOnlyList<CsvShipmentRecord> _shipments = Array.Empty<CsvShipmentRecord>();
    private List<CsvShipmentRecord> _simulatedShipments = new();
    private IReadOnlyList<AuditEvent> _auditEvents = Array.Empty<AuditEvent>();
    private ShipmentRiskProfile _profile = new();
    
    private int _totalShipments = 0;
    private int _activeShipments = 0;
    private double _fleetPercent = 0;
    private double _highestRisk = 0;
    private double _currentRiskPercent = 0;
    private bool _isCritical = false;
    private double _lastLatencyMs = 12;
    private string _lastLoadTime = "--:--:--";

    private System.Threading.Timer? _timer;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        _timer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadDataAsync();
                StateHasChanged();
            });
        }, null, 5000, 5000);
    }

    private async Task LoadDataAsync()
    {
        var realShipments = await DataService.GetShipmentsAsync();
        
        var combined = new List<CsvShipmentRecord>(_simulatedShipments);
        combined.AddRange(realShipments);
        _shipments = combined;
        
        _fleetMetrics = await DataService.GetFleetMetricsAsync();
        _auditEvents = await DataService.GetAuditEventsAsync();

        _totalShipments = _shipments.Count;
        _activeShipments = _totalShipments > 0 ? (int)(_totalShipments * 0.9) : 0; // Simulated
        _fleetPercent = _totalShipments > 0 ? (_activeShipments * 100.0 / _totalShipments) : 0;
        
        _highestRisk = _shipments.Any() ? _shipments.Max(s => s.RiskPercent) : 0;
        _currentRiskPercent = _fleetMetrics.Slope * 100;
        _isCritical = _fleetMetrics.IsCritical;
        _lastLoadTime = DateTime.Now.ToString("HH:mm:ss");
    }

    private async Task ReloadData()
    {
        await LoadDataAsync();
    }

    private void SelectShipment(CsvShipmentRecord shipment)
    {
        // For hovering row UX future use
    }

    private async Task HandleWhatIfAnalysis(ShipmentRiskProfile profile)
    {
        var sw = Stopwatch.StartNew();
        
        var metrics = await DataService.GetFleetMetricsAsync();
        
        double riskModifier = profile.Insulation == InsulationType.Active ? 0.85 : 1.15;
        
        _currentRiskPercent = Math.Min(100, metrics.Slope * 100 * riskModifier);
        _isCritical = _currentRiskPercent > 75;
        
        string status = _isCritical ? "CRITICAL" : _currentRiskPercent > 50 ? "ELEVATED" : "NOMINAL";
        string insString = profile.Insulation == InsulationType.Active ? "ACTIVE" : "PASSIVE";

        var newRecord = new CsvShipmentRecord(
            $"SIM-{DateTime.UtcNow:HHmmss}",
            string.IsNullOrWhiteSpace(profile.CarrierName) ? "SIMULATED_CARRIER" : profile.CarrierName,
            insString,
            profile.RouteId,
            _currentRiskPercent,
            profile.ExternalTempForecast,
            status,
            "WHAT_IF_INJECTION",
            _currentRiskPercent / 100.0,
            _isCritical ? 15 : 120
        );

        _simulatedShipments.Insert(0, newRecord);
        
        var listShipments = new List<CsvShipmentRecord>(_simulatedShipments);
        var realShipments = await DataService.GetShipmentsAsync();
        listShipments.AddRange(realShipments);
        
        _shipments = listShipments;
        _totalShipments = _shipments.Count;

        sw.Stop();
        _lastLatencyMs = sw.Elapsed.TotalMilliseconds;
        
        var list = _auditEvents.ToList();
        list.Insert(0, new AuditEvent(
            Timestamp: DateTime.UtcNow,
            EventName: "WHAT_IF_SIMULATION_EXECUTED",
            ActionTaken: $"Insulation={profile.Insulation} Risk={_currentRiskPercent:F1}%",
            DigitalSignature: Guid.NewGuid().ToString("N")[..12],
            Severity: _isCritical ? EventSeverity.Critical : EventSeverity.Info
        ));
        _auditEvents = list.Take(20).ToList();
        
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
