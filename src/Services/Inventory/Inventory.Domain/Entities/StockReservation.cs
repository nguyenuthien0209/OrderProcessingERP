using Common;

namespace Inventory.Domain.Entities;

/// <summary>Records what was reserved for a given order so a later compensating release knows exactly how much to give back.</summary>
public class StockReservation : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ReservedOnUtc { get; private set; }

    private StockReservation() { } // EF Core

    public static StockReservation Create(Guid orderId, Guid productId, int quantity) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        ProductId = productId,
        Quantity = quantity,
        ReservedOnUtc = DateTime.UtcNow
    };
}
