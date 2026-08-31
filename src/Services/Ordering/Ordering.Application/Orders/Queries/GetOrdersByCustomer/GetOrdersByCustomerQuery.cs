using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Orders.Queries.GetOrderById;

namespace Ordering.Application.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<List<OrderDto>>;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    private readonly IOrderingDbContext _dbContext;

    public GetOrdersByCustomerQueryHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        return orders.Select(GetOrderByIdQueryHandler.Map).ToList();
    }
}
