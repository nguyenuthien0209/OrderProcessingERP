using MediatR;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;

namespace Shipping.Application.Shipments.Queries.GetShipmentByOrderId;

public record ShipmentDto(Guid Id, Guid OrderId, string Carrier, string TrackingNumber, DateTime ShippedOnUtc);

public record GetShipmentByOrderIdQuery(Guid OrderId) : IRequest<ShipmentDto?>;

public class GetShipmentByOrderIdQueryHandler : IRequestHandler<GetShipmentByOrderIdQuery, ShipmentDto?>
{
    private readonly IShippingDbContext _dbContext;

    public GetShipmentByOrderIdQueryHandler(IShippingDbContext dbContext) => _dbContext = dbContext;

    public async Task<ShipmentDto?> Handle(GetShipmentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId, cancellationToken);

        return shipment is null
            ? null
            : new ShipmentDto(shipment.Id, shipment.OrderId, shipment.Carrier, shipment.TrackingNumber, shipment.ShippedOnUtc);
    }
}
