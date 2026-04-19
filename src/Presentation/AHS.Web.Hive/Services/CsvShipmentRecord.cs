namespace AHS.Web.Hive.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Quality Rules", "CA1515:Consider making types internal", Justification = "Used for data transfer in UI components.")]
public record CsvShipmentRecord(
    string  Id,
    string  CarrierName,
    string  Insulation,      // "PASSIVE" | "ACTIVE"
    string  RouteId,
    double  RiskPercent,     // e.g. 58.1 (stored as 58.1, display as "58,1%")
    double  ExternalTemp,
    string  Status,          // "ELEVATED" | "CRITICAL" | "NOMINAL"
    string  Insight,         // "INSULATION_TYPE" | "THERMAL_EXPOSURE" etc.
    double  Slope,
    double  TimeToFailureMin
);
