using FluentAssertions;
using Payments.Domain.Entities;

namespace Payments.Domain.Tests;

public class PendingOrderTests
{
    [Fact]
    public void Create_UsesOrderIdAsIdentityAndSetsTotal()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var pendingOrder = PendingOrder.Create(orderId, customerId, totalAmount: 150.00m);

        pendingOrder.Id.Should().Be(orderId);
        pendingOrder.CustomerId.Should().Be(customerId);
        pendingOrder.TotalAmount.Should().Be(150.00m);
    }
}
