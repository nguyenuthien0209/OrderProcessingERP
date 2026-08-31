using Common;

namespace Shipping.Domain.Entities;

public class Shipment : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public string Carrier { get; private set; } = default!;
    public string TrackingNumber { get; private set; } = default!;
    public DateTime ShippedOnUtc { get; private set; }

    private Shipment() { } // EF Core

    public static Shipment Create(Guid orderId, string carrier, string trackingNumber) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        Carrier = carrier,
        TrackingNumber = trackingNumber,
        ShippedOnUtc = DateTime.UtcNow
    };
}
