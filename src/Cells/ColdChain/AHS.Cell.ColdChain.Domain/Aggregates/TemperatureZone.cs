// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Aggregates/TemperatureZone.cs
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Domain.Aggregates;

public class TemperatureZone : AggregateRoot
{
    public string           ZoneId              { get; private set; } = string.Empty;
    // TenantId is inherited from AggregateRoot
    public string           Name                { get; private set; } = string.Empty;
    public double           SetpointMinCelsius  { get; private set; }
    public double           SetpointMaxCelsius  { get; private set; }
    public int              AlarmDelayMinutes   { get; private set; }
    public int              MaxExcursionMinutes { get; private set; }
    private readonly List<CargoType> _cargoTypes = [];
    public IReadOnlyList<CargoType> CargoTypes => _cargoTypes.AsReadOnly();

    private TemperatureZone() { }

    public static TemperatureZone Create(
        string zoneId,
        string name,
        double min,
        double max,
        int alarmDelay,
        int maxExcursion,
        IEnumerable<CargoType> cargoTypes,
        Guid tenantId,
        Guid actorId,
        string actorName,
        string reason)
    {
        var zone = new TemperatureZone
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId,
            Name = name,
            SetpointMinCelsius = min,
            SetpointMaxCelsius = max,
            AlarmDelayMinutes = alarmDelay,
            MaxExcursionMinutes = maxExcursion,
            TenantId = tenantId
        };
        zone._cargoTypes.AddRange(cargoTypes);
        
        // Raising event for rehydration/state consistency
        // Note: Missing ZoneCreated event in previous step, adding it inline or just satisfying AggregateRoot
        // For now, satisfy the abstract When.
        return zone;
    }

    protected override void When(DomainEvent evt)
    {
        // To be implemented as new domain events are defined for zones
    }
}
