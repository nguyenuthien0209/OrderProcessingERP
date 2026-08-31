namespace Ordering.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Confirmed = 2,
    Shipped = 3,
    Cancelled = 4
}
