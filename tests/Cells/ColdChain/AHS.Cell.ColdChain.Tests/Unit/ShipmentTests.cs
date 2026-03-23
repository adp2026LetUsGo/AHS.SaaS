// tests/Cells/ColdChain/AHS.Cell.ColdChain.Tests/Unit/ShipmentTests.cs
using AHS.Cell.ColdChain.Domain.Aggregates;
using AHS.Cell.ColdChain.Domain.Enums;
using AHS.Cell.ColdChain.Domain.Events;
using FluentAssertions;
using Xunit;

namespace AHS.Cell.ColdChain.Tests.Unit;

public class ShipmentTests
{
    [Fact]
    public void Create_valid_shipment_raises_ShipmentCreated_event()
    {
        var tenantId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var shipment = Shipment.Create(
            CargoType.Pharmaceutical,
            InsulationType.Active,
            "London",
            "New York",
            DateTimeOffset.UtcNow.AddDays(1),
            tenantId,
            actorId,
            "John Doe",
            "New shipment registration");

        shipment.UncommittedEvents.Should().ContainSingle(e => e is ShipmentCreated);
        shipment.Status.Should().Be(ShipmentStatus.Draft);
    }

    [Fact]
    public void RecordExcursion_on_sealed_shipment_throws()
    {
        var shipment = CreateSealedShipment();

        Action act = () => shipment.RecordExcursion("S1", "Z1", 10.0, 2.0, 8.0, ExcursionSeverity.Critical, Guid.NewGuid(), "System", "Test");

        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot record excursion on a sealed shipment.");
    }

    private Shipment CreateSealedShipment()
    {
        var shipment = Shipment.Create(CargoType.Pharmaceutical, InsulationType.Active, "A", "B", DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid(), "Admin", "Init");
        shipment.Seal(ShipmentStatus.Compliant, 5.0, QualityDecision.Accept, Guid.NewGuid(), "Admin", "Sealing");
        shipment.ClearUncommitted();
        return shipment;
    }
}
