using MediatR;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Orders.Commands.MarkOrderShipped;

/// <summary>Reflects the shipment Shipping already created and announced via OrderShippedIntegrationEvent.</summary>
public record MarkOrderShippedCommand(Guid OrderId, string Carrier, string TrackingNumber) : IRequest;

public class MarkOrderShippedCommandHandler : IRequestHandler<MarkOrderShippedCommand>
{
    private readonly IOrderingDbContext _dbContext;

    public MarkOrderShippedCommandHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(MarkOrderShippedCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([request.OrderId], cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.MarkShipped(request.Carrier, request.TrackingNumber);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
