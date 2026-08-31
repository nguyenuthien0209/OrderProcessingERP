namespace EventBus.Contracts;

/// <summary>Published by Ordering after an order and its outbox row are committed. Starts the saga.</summary>
public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
}

/// <summary>Published by Ordering once both inventory has been reserved and payment has been authorized.</summary>
public record OrderConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Published by Ordering when the saga cannot complete (stock unavailable or payment declined).
/// Consumed by Inventory as the trigger to release any stock it had reserved for this order.
/// </summary>
public record OrderCancelledIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
}

/// <summary>Published by Shipping once a shipment has been created for a confirmed order.</summary>
public record OrderShippedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Carrier { get; init; } = default!;
    public string TrackingNumber { get; init; } = default!;
}
