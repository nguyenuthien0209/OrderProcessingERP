using FluentAssertions;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Tests;

public class StockItemTests
{
    [Fact]
    public void Create_SetsQuantityOnHandAndZeroReserved()
    {
        var productId = Guid.NewGuid();

        var stockItem = StockItem.Create(productId, quantityOnHand: 100);

        stockItem.ProductId.Should().Be(productId);
        stockItem.QuantityOnHand.Should().Be(100);
        stockItem.QuantityReserved.Should().Be(0);
        stockItem.QuantityAvailable.Should().Be(100);
    }

    [Fact]
    public void TryReserve_WithSufficientStock_ReservesAndReturnsTrue()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 10);

        var reserved = stockItem.TryReserve(4);

        reserved.Should().BeTrue();
        stockItem.QuantityReserved.Should().Be(4);
        stockItem.QuantityAvailable.Should().Be(6);
    }

    [Fact]
    public void TryReserve_WithInsufficientStock_ReturnsFalseAndDoesNotMutate()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 3);

        var reserved = stockItem.TryReserve(4);

        reserved.Should().BeFalse();
        stockItem.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void TryReserve_WithNonPositiveQuantity_Throws()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 10);

        var act = () => stockItem.TryReserve(0);

        act.Should().Throw<ArgumentException>().WithParameterName("quantity");
    }

    [Fact]
    public void Release_ReducesReservedQuantity()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 10);
        stockItem.TryReserve(5);

        stockItem.Release(2);

        stockItem.QuantityReserved.Should().Be(3);
        stockItem.QuantityAvailable.Should().Be(7);
    }

    [Fact]
    public void Release_MoreThanReserved_ClampsToZero()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 10);
        stockItem.TryReserve(3);

        stockItem.Release(100);

        stockItem.QuantityReserved.Should().Be(0);
        stockItem.QuantityAvailable.Should().Be(10);
    }

    [Fact]
    public void Release_WithNonPositiveQuantity_Throws()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), quantityOnHand: 10);

        var act = () => stockItem.Release(-1);

        act.Should().Throw<ArgumentException>().WithParameterName("quantity");
    }
}
