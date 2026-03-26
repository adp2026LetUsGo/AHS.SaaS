// tests/Cells/Xinfer/AHS.Cell.Xinfer.Tests/Unit/ShipmentTests.cs
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Cell.Xinfer.Domain.Events;
using FluentAssertions;
using Xunit;

namespace AHS.Cell.Xinfer.Tests.Unit;

public class ShipmentTests
{
    [Fact]
    public void CreateValidShipmentRaisesShipmentCreatedEvent()
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
    public void RecordExcursionOnSealedShipmentThrows()
    {
        var shipment = CreateSealedShipment();

        Action act = () => shipment.RecordExcursion("S1", "Z1", 10.0, 2.0, 8.0, ExcursionSeverity.Critical, Guid.NewGuid(), "System", "Test");

        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot record excursion on a sealed shipment.");
    }

    private static Shipment CreateSealedShipment()
    {
        var shipment = Shipment.Create(CargoType.Pharmaceutical, InsulationType.Active, "A", "B", DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid(), "Admin", "Init");
        shipment.Seal(ShipmentStatus.Compliant, 5.0, QualityDecision.Accept, Guid.NewGuid(), "Admin", "Sealing");
        shipment.ClearUncommitted();
        return shipment;
    }
}
