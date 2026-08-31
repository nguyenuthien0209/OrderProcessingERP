namespace EventBus.Contracts;

/// <summary>
/// Base type for every message that crosses a service boundary over RabbitMQ.
/// <see cref="CorrelationId"/> is always the OrderId, so every hop of a given order's
/// saga can be traced end to end across services with a single id.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
