namespace AHS.Cell.Xinfer.UI.Models;

internal sealed class ShipmentRecord
{
    public string RouteId { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public double TransitTimeHrs { get; set; }
    public double ExternalTempAvg { get; set; }
    public string PackagingType { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public int ShipmentSize { get; set; }
    public bool DelayFlag { get; set; }
    public bool TempExcursion { get; set; }

    // UI Properties
    public double RiskScore { get; set; }
    public bool IsProcessed { get; set; }
    public string Hash { get; set; } = string.Empty;
}
