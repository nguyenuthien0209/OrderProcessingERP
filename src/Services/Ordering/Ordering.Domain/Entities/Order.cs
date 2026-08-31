using Common;
using Ordering.Domain.Enums;

namespace Ordering.Domain.Entities;

public class Order : Entity<Guid>
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.UnitPrice * i.Quantity);

    private Order() { } // EF Core

    public static Order Create(Guid customerId, IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new ArgumentException("An order must contain at least one item.", nameof(items));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow
        };

        foreach (var item in itemList)
            order._items.Add(OrderItem.Create(order.Id, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice));

        return order;
    }

    public void MarkAwaitingPayment()
    {
        EnsureStatus(OrderStatus.Pending);
        Status = OrderStatus.AwaitingPayment;
    }

    public void Confirm()
    {
        EnsureStatus(OrderStatus.AwaitingPayment);
        Status = OrderStatus.Confirmed;
    }

    /// <summary>Valid from any pre-shipment status: inventory reservation and payment authorization can each fail independently.</summary>
    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Cancelled)
            throw new InvalidOperationException($"Order {Id} cannot be cancelled from status {Status}.");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
    }

    public void MarkShipped(string carrier, string trackingNumber)
    {
        EnsureStatus(OrderStatus.Confirmed);
        Status = OrderStatus.Shipped;
        Carrier = carrier;
        TrackingNumber = trackingNumber;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Order {Id} expected status {expected} but was {Status}.");
    }
}
