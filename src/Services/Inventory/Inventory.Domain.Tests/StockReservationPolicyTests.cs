using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;

namespace Inventory.Domain.Tests;

public class StockReservationPolicyTests
{
    [Fact]
    public void TryReserveAll_WhenEveryLineHasStock_ReservesAllAndReturnsTrue()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var stockByProductId = new Dictionary<Guid, StockItem>
        {
            [productA] = StockItem.Create(productA, quantityOnHand: 10),
            [productB] = StockItem.Create(productB, quantityOnHand: 5)
        };
        var requestedItems = new[] { (productA, 4), (productB, 5) };

        var succeeded = StockReservationPolicy.TryReserveAll(stockByProductId, requestedItems, out var failureReason);

        succeeded.Should().BeTrue();
        failureReason.Should().BeNull();
        stockByProductId[productA].QuantityReserved.Should().Be(4);
        stockByProductId[productB].QuantityReserved.Should().Be(5);
    }

    [Fact]
    public void TryReserveAll_WhenOneLineHasNoStockRecord_FailsWithoutReservingAnything()
    {
        var productA = Guid.NewGuid();
        var missingProduct = Guid.NewGuid();
        var stockByProductId = new Dictionary<Guid, StockItem>
        {
            [productA] = StockItem.Create(productA, quantityOnHand: 10)
        };
        var requestedItems = new[] { (productA, 4), (missingProduct, 1) };

        var succeeded = StockReservationPolicy.TryReserveAll(stockByProductId, requestedItems, out var failureReason);

        succeeded.Should().BeFalse();
        failureReason.Should().Contain(missingProduct.ToString());
        stockByProductId[productA].QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void TryReserveAll_WhenOneLineHasInsufficientStock_FailsWithoutReservingAnything()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var stockByProductId = new Dictionary<Guid, StockItem>
        {
            [productA] = StockItem.Create(productA, quantityOnHand: 10),
            [productB] = StockItem.Create(productB, quantityOnHand: 2)
        };
        var requestedItems = new[] { (productA, 4), (productB, 5) };

        var succeeded = StockReservationPolicy.TryReserveAll(stockByProductId, requestedItems, out var failureReason);

        succeeded.Should().BeFalse();
        failureReason.Should().NotBeNull();
        stockByProductId[productA].QuantityReserved.Should().Be(0);
        stockByProductId[productB].QuantityReserved.Should().Be(0);
    }
}
