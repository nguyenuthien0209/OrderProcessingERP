using FluentAssertions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Authorized_SetsAuthorizedStatusAndAmount()
    {
        var orderId = Guid.NewGuid();

        var payment = Payment.Authorized(orderId, 99.95m);

        payment.OrderId.Should().Be(orderId);
        payment.Amount.Should().Be(99.95m);
        payment.Status.Should().Be(PaymentStatus.Authorized);
        payment.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Failed_SetsFailedStatusAndReason()
    {
        var orderId = Guid.NewGuid();

        var payment = Payment.Failed(orderId, 50.00m, "Card declined");

        payment.OrderId.Should().Be(orderId);
        payment.Amount.Should().Be(50.00m);
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Card declined");
    }
}
