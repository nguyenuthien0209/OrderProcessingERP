namespace EventBus.Contracts;

/// <summary>Published by Payments once funds have been authorized for the order total.</summary>
public record PaymentAuthorizedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
}

/// <summary>Published by Payments when authorization is declined. Fails the order and triggers compensation.</summary>
public record PaymentFailedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
}
