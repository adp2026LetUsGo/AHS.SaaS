// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Aggregates/Shipment.cs
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Enums;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;
using AHS.Cell.Xinfer.Domain.Events;

namespace AHS.Cell.Xinfer.Domain.Aggregates;

public class Shipment : AggregateRoot
{
    // TenantId is inherited from AggregateRoot
    public CargoType       CargoType           { get; private set; }
    public InsulationType  InsulationType      { get; private set; }
    public ShipmentStatus  Status              { get; private set; }
    public double?         LastRiskScore       { get; private set; }
    public double?         LastAccuracyScore   { get; private set; }
    public double?         LastReliabilityScore { get; private set; }
    public string         OriginLocation      { get; private set; } = string.Empty;
    public string         DestinationLocation { get; private set; } = string.Empty;
    public DateTimeOffset PlannedDeparture    { get; private set; }
    public DateTimeOffset? ActualDeparture     { get; private set; }
    public DateTimeOffset? SealedAt            { get; private set; }
    private readonly List<Guid> _excursions = [];
    public IReadOnlyList<Guid> Excursions => _excursions.AsReadOnly();
    public bool           IsSealed            => SealedAt.HasValue;

    private Shipment() { }
    
    public static new Shipment Rehydrate(IEnumerable<DomainEvent> history)
    {
        var shipment = new Shipment();
        ((AggregateRoot)shipment).Rehydrate(history);
        return shipment;
    }

    public static Shipment Create(
        CargoType cargoType,
        InsulationType insulationType,
        string origin,
        string destination,
        DateTimeOffset plannedDeparture,
        Guid tenantId,
        Guid actorId,
        string actorName,
        string reason)
    {
        var id = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = id,
            TenantId = tenantId,
            CargoType = cargoType,
            InsulationType = insulationType,
            Status = ShipmentStatus.Draft,
            OriginLocation = origin,
            DestinationLocation = destination,
            PlannedDeparture = plannedDeparture
        };

        shipment.Apply(new ShipmentCreated(
            id, cargoType, insulationType, origin, destination, plannedDeparture, tenantId));

        return shipment;
    }

    public void RecordExcursion(
        string sensorId,
        string zoneId,
        double celsius,
        double min,
        double max,
        ExcursionSeverity severity,
        Guid actorId,
        string actorName,
        string reason)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot record excursion on a sealed shipment.");

        var occurredAt = DateTimeOffset.UtcNow;
        var evt = new TemperatureExcursionRecorded(
            Id, sensorId, zoneId, celsius, min, max, occurredAt, severity, TenantId);

        _excursions.Add(Guid.NewGuid()); // Simulated event ID for state tracking
        Status = ShipmentStatus.UnderReview;
        
        Apply(evt);
    }

    public void ResolveExcursion(
        Guid excursionEventId,
        string note,
        Guid actorId,
        string actorName,
        string reason)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot resolve excursion on a sealed shipment.");

        var resolvedAt = DateTimeOffset.UtcNow;
        Apply(new ExcursionResolved(Id, excursionEventId, resolvedAt, note, TenantId));

        // Logic to transition back to Active if all resolved would go here
        Status = ShipmentStatus.Active; 
    }

    public void RecordPrediction(double riskScore, double accuracyScore, double reliabilityScore)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot record prediction on a sealed shipment.");
        
        Apply(new PredictionRecorded(Id, riskScore, accuracyScore, reliabilityScore, TenantId));
    }

    public void Seal(
        ShipmentStatus finalStatus,
        double mktCelsius,
        QualityDecision qualityDecision,
        Guid actorId,
        string actorName,
        string reason)
    {
        if (IsSealed) throw new InvalidOperationException("Shipment is already sealed.");
        if (Status == ShipmentStatus.UnderReview) throw new InvalidOperationException("Cannot seal with unresolved excursions.");

        Status = finalStatus;
        SealedAt = DateTimeOffset.UtcNow;

        Apply(new ShipmentSealed(Id, finalStatus, mktCelsius, qualityDecision, TenantId));
    }

    protected override void When(DomainEvent evt)
    {
        switch (evt)
        {
            case ShipmentCreated e:
                Id = e.ShipmentId;
                TenantId = e.TenantId;
                CargoType = e.CargoType;
                InsulationType = e.InsulationType;
                OriginLocation = e.OriginLocation;
                DestinationLocation = e.DestinationLocation;
                PlannedDeparture = e.PlannedDeparture;
                Status = ShipmentStatus.Draft;
                break;
            case TemperatureExcursionRecorded _:
                Status = ShipmentStatus.UnderReview;
                break;
            case ExcursionResolved _:
                Status = ShipmentStatus.Active;
                break;
            case ShipmentSealed e:
                Status = e.FinalStatus;
                SealedAt = DateTimeOffset.UtcNow;
                break;
            case PredictionRecorded e:
                LastRiskScore = e.RiskScore;
                LastAccuracyScore = e.AccuracyScore;
                LastReliabilityScore = e.ReliabilityScore;
                break;
        }
    }
}
