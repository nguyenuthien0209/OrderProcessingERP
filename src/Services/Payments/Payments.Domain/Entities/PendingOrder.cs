using Common;

namespace Payments.Domain.Entities;

/// <summary>
/// Payments' own local copy of the order total, materialized from OrderCreatedIntegrationEvent.
/// Charging happens later, triggered by InventoryReserved, so the amount has to be on hand already —
/// this is the standard "replicate what you need via events" alternative to a synchronous call back to Ordering.
/// </summary>
public class PendingOrder : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }

    private PendingOrder() { } // EF Core

    public static PendingOrder Create(Guid orderId, Guid customerId, decimal totalAmount) => new()
    {
        Id = orderId,
        CustomerId = customerId,
        TotalAmount = totalAmount,
        CreatedOnUtc = DateTime.UtcNow
    };
}
