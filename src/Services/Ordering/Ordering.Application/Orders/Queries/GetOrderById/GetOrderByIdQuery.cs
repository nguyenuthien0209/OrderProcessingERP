using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;

namespace Ordering.Application.Orders.Queries.GetOrderById;

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedOnUtc,
    string? CancellationReason,
    string? Carrier,
    string? TrackingNumber,
    List<OrderItemDto> Items);

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderingDbContext _dbContext;

    public GetOrderByIdQueryHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return order is null ? null : Map(order);
    }

    internal static OrderDto Map(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.TotalAmount,
        order.CreatedOnUtc,
        order.CancellationReason,
        order.Carrier,
        order.TrackingNumber,
        order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());
}
