using Common;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class Payment : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ProcessedOnUtc { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { } // EF Core

    public static Payment Authorized(Guid orderId, decimal amount) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        Amount = amount,
        Status = PaymentStatus.Authorized,
        ProcessedOnUtc = DateTime.UtcNow
    };

    public static Payment Failed(Guid orderId, decimal amount, string reason) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        Amount = amount,
        Status = PaymentStatus.Failed,
        ProcessedOnUtc = DateTime.UtcNow,
        FailureReason = reason
    };
}
