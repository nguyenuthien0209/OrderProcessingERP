using Common.Outbox;
using EventBus.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;
using Shipping.Domain.Entities;

namespace Shipping.Application.Shipments.Commands.CreateShipment;

/// <summary>Triggered by OrderConfirmed — the last hop of the happy path. Publishes OrderShipped when done.</summary>
public record CreateShipmentCommand(Guid OrderId) : IRequest;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand>
{
    private readonly IShippingDbContext _dbContext;

    public CreateShipmentCommandHandler(IShippingDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var alreadyShipped = await _dbContext.Shipments.AnyAsync(s => s.OrderId == request.OrderId, cancellationToken);
        if (alreadyShipped)
            return;

        const string carrier = "Standard Carrier";
        var trackingNumber = $"TRK-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

        var shipment = Shipment.Create(request.OrderId, carrier, trackingNumber);
        _dbContext.Shipments.Add(shipment);

        _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new OrderShippedIntegrationEvent
        {
            CorrelationId = request.OrderId,
            OrderId = request.OrderId,
            Carrier = carrier,
            TrackingNumber = trackingNumber
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
