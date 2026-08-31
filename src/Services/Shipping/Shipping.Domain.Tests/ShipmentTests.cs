using FluentAssertions;
using Shipping.Domain.Entities;

namespace Shipping.Domain.Tests;

public class ShipmentTests
{
    [Fact]
    public void Create_SetsOrderIdCarrierAndTrackingNumber()
    {
        var orderId = Guid.NewGuid();

        var shipment = Shipment.Create(orderId, "UPS", "1Z999AA10123456784");

        shipment.OrderId.Should().Be(orderId);
        shipment.Carrier.Should().Be("UPS");
        shipment.TrackingNumber.Should().Be("1Z999AA10123456784");
        shipment.ShippedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
