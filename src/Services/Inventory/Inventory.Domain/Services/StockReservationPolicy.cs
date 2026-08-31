using Inventory.Domain.Entities;

namespace Inventory.Domain.Services;

/// <summary>
/// The "reserve every line or none" business rule, kept out of the Application layer so it can be
/// reasoned about (and unit tested) without EF Core, MediatR, or the outbox.
/// </summary>
public static class StockReservationPolicy
{
    /// <summary>
    /// Validates every requested line against the given stock before committing any reservation, so the
    /// outcome is genuinely all-or-nothing — no reliance on discarding partially-mutated tracked entities.
    /// </summary>
    public static bool TryReserveAll(
        IReadOnlyDictionary<Guid, StockItem> stockByProductId,
        IReadOnlyCollection<(Guid ProductId, int Quantity)> requestedItems,
        out string? failureReason)
    {
        foreach (var (productId, quantity) in requestedItems)
        {
            if (!stockByProductId.TryGetValue(productId, out var stockItem))
            {
                failureReason = $"No stock record for product {productId}.";
                return false;
            }

            if (stockItem.QuantityAvailable < quantity)
            {
                failureReason = $"Insufficient stock for product {productId}: requested {quantity}, available {stockItem.QuantityAvailable}.";
                return false;
            }
        }

        foreach (var (productId, quantity) in requestedItems)
            stockByProductId[productId].TryReserve(quantity);

        failureReason = null;
        return true;
    }
}
