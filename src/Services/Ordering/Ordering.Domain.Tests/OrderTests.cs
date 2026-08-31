using FluentAssertions;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;

namespace Ordering.Domain.Tests;

public class OrderTests
{
    private static readonly (Guid ProductId, string ProductName, int Quantity, decimal UnitPrice) OneItem =
        (Guid.NewGuid(), "Wireless Mouse", 2, 25.00m);

    [Fact]
    public void Create_WithValidItems_SetsPendingStatusAndComputesTotal()
    {
        var customerId = Guid.NewGuid();

        var order = Order.Create(customerId, new[] { OneItem });

        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(50.00m);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        var act = () => Order.Create(Guid.Empty, new[] { OneItem });

        act.Should().Throw<ArgumentException>().WithParameterName("customerId");
    }

    [Fact]
    public void Create_WithNoItems_Throws()
    {
        var act = () => Order.Create(Guid.NewGuid(), Array.Empty<(Guid, string, int, decimal)>());

        act.Should().Throw<ArgumentException>().WithParameterName("items");
    }

    [Fact]
    public void Create_WithNonPositiveQuantity_Throws()
    {
        var invalidItem = (Guid.NewGuid(), "Bad Item", 0, 10.00m);

        var act = () => Order.Create(Guid.NewGuid(), new[] { invalidItem });

        act.Should().Throw<ArgumentException>().WithParameterName("quantity");
    }

    [Fact]
    public void MarkAwaitingPayment_FromPending_TransitionsStatus()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });

        order.MarkAwaitingPayment();

        order.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    [Fact]
    public void MarkAwaitingPayment_FromNonPending_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        order.MarkAwaitingPayment();

        var act = order.MarkAwaitingPayment;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirm_FromAwaitingPayment_TransitionsStatus()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        order.MarkAwaitingPayment();

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_FromPending_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });

        var act = order.Confirm;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.Confirmed)]
    public void Cancel_FromAnyPreShipmentStatus_SetsCancelledWithReason(OrderStatus fromStatus)
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        AdvanceTo(order, fromStatus);

        order.Cancel("Customer requested cancellation.");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer requested cancellation.");
    }

    [Fact]
    public void Cancel_WhenAlreadyShipped_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        AdvanceTo(order, OrderStatus.Shipped);

        var act = () => order.Cancel("too late");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        order.Cancel("first reason");

        var act = () => order.Cancel("second reason");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkShipped_FromConfirmed_SetsCarrierAndTracking()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });
        AdvanceTo(order, OrderStatus.Confirmed);

        order.MarkShipped("UPS", "1Z999AA10123456784");

        order.Status.Should().Be(OrderStatus.Shipped);
        order.Carrier.Should().Be("UPS");
        order.TrackingNumber.Should().Be("1Z999AA10123456784");
    }

    [Fact]
    public void MarkShipped_FromPending_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { OneItem });

        var act = () => order.MarkShipped("UPS", "1Z999AA10123456784");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TotalAmount_SumsAcrossMultipleItems()
    {
        var items = new[]
        {
            (Guid.NewGuid(), "Item A", 2, 10.00m),
            (Guid.NewGuid(), "Item B", 3, 5.50m)
        };

        var order = Order.Create(Guid.NewGuid(), items);

        order.TotalAmount.Should().Be(2 * 10.00m + 3 * 5.50m);
    }

    private static void AdvanceTo(Order order, OrderStatus status)
    {
        if (status is OrderStatus.AwaitingPayment or OrderStatus.Confirmed or OrderStatus.Shipped)
            order.MarkAwaitingPayment();
        if (status is OrderStatus.Confirmed or OrderStatus.Shipped)
            order.Confirm();
        if (status is OrderStatus.Shipped)
            order.MarkShipped("UPS", "1Z999AA10123456784");
    }
}
