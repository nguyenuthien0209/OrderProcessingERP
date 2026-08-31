namespace EventBus.Contracts;

/// <summary>Published by Inventory once stock for every line item on the order has been reserved.</summary>
public record InventoryReservedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
}

/// <summary>Published by Inventory when one or more line items could not be reserved. Fails the order.</summary>
public record InventoryReservationFailedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
}
