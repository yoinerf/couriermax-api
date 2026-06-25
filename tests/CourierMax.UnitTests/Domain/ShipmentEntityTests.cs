using CourierMax.Domain.Enums;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CourierMax.UnitTests.Domain;

public class ShipmentEntityTests
{
    private static Shipment CreateShipment(ServiceType serviceType = ServiceType.Standard) =>
        new(
            trackingCode: "CM-00000001",
            serviceType: serviceType,
            packageType: PackageType.Package,
            packageWeight: 5m,
            packageDimensions: "30x20x15",
            packageVolumeM3: 0.009m,
            senderName: "Juan Pérez",
            senderPhone: "3001234567",
            senderAddress: "Calle 1 # 2-3",
            recipientName: "María López",
            recipientPhone: "3109876543",
            recipientAddress: "Carrera 4 # 5-6",
            originCityId: 1,
            destinationCityId: 2
        );

    [Fact]
    public void NewShipment_HasCreatedStatus()
    {
        var shipment = CreateShipment();
        shipment.Status.Should().Be(ShipmentStatus.Created);
    }

    [Theory]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Assigned)]
    [InlineData(ShipmentStatus.Assigned, ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Delivered)]
    public void ValidTransitions_ShouldSucceed(ShipmentStatus from, ShipmentStatus to)
    {
        var shipment = CreateShipment();

        // Avanzar al estado 'from'
        AdvanceTo(shipment, from);

        // La siguiente transición debe tener éxito
        var act = () => shipment.TransitionTo(to, "system");
        act.Should().NotThrow();
        shipment.Status.Should().Be(to);
    }

    [Fact]
    public void CancelFromCreated_ShouldSucceed_WithValidReason()
    {
        var shipment = CreateShipment();

        var act = () => shipment.TransitionTo(ShipmentStatus.Cancelled, "admin", "Client requested cancellation");
        act.Should().NotThrow();
        shipment.Status.Should().Be(ShipmentStatus.Cancelled);
    }

    [Fact]
    public void CancelDeliveredShipment_ShouldThrow()
    {
        var shipment = CreateShipment();
        AdvanceTo(shipment, ShipmentStatus.Delivered);

        var act = () => shipment.TransitionTo(ShipmentStatus.Cancelled, "admin", "Too late");
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void CancelWithShortReason_ShouldThrow()
    {
        var shipment = CreateShipment();

        var act = () => shipment.TransitionTo(ShipmentStatus.Cancelled, "admin", "No");
        act.Should().Throw<DomainException>().Which.Code.Should().Be("CANCELLATION_REASON_REQUIRED");
    }

    [Fact]
    public void CancelWithoutReason_ShouldThrow()
    {
        var shipment = CreateShipment();

        var act = () => shipment.TransitionTo(ShipmentStatus.Cancelled, "admin");
        act.Should().Throw<DomainException>().Which.Code.Should().Be("CANCELLATION_REASON_REQUIRED");
    }

    [Fact]
    public void InvalidTransition_CreatedToDelivered_ShouldThrow()
    {
        var shipment = CreateShipment();

        var act = () => shipment.TransitionTo(ShipmentStatus.Delivered, "system");
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void TransitionTo_ShouldRecordHistory()
    {
        var shipment = CreateShipment();
        // El envío inicia en 'Created'; transición a Assigned y luego a InTransit
        shipment.TransitionTo(ShipmentStatus.Assigned, "operator");
        shipment.TransitionTo(ShipmentStatus.InTransit, "driver");

        shipment.History.Should().HaveCount(2);
        shipment.History.Last().NewStatus.Should().Be(ShipmentStatus.InTransit);
        shipment.History.Last().PreviousStatus.Should().Be(ShipmentStatus.Assigned);
    }

    [Fact]
    public void DeliveredShipment_ShouldSetDeliveredAt()
    {
        var shipment = CreateShipment();
        AdvanceTo(shipment, ShipmentStatus.Delivered);

        shipment.DeliveredAt.Should().NotBeNull();
    }

    // Auxiliar: avanza el envío por los estados sin disparar lógica de negocio externa
    private static void AdvanceTo(Shipment shipment, ShipmentStatus target)
    {
        var chain = new[]
        {
            ShipmentStatus.Created,
            ShipmentStatus.Assigned,
            ShipmentStatus.InTransit,
            ShipmentStatus.Delivered
        };

        foreach (var status in chain)
        {
            if (shipment.Status == target) break;
            if (status <= shipment.Status) continue;
            shipment.TransitionTo(status, "test");
        }
    }
}
