using Common;

namespace Inventory.Domain.Entities;

public class StockItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    private StockItem() { } // EF Core

    public static StockItem Create(Guid productId, int quantityOnHand) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        QuantityOnHand = quantityOnHand,
        QuantityReserved = 0
    };

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (QuantityAvailable < quantity) return false;

        QuantityReserved += quantity;
        return true;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
    }
}
