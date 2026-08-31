using Common.Outbox;
using EventBus.Contracts;
using FluentValidation;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Stock.Commands.ReserveStock;

public record ReserveStockItemDto(Guid ProductId, int Quantity);

public record ReserveStockCommand(Guid OrderId, List<ReserveStockItemDto> Items) : IRequest;

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}

/// <summary>
/// Orchestration only: loads stock, delegates the all-or-nothing reservation rule to
/// <see cref="StockReservationPolicy"/>, then persists and stages the outcome event.
/// Idempotent by design — a redelivered OrderCreated for an order already reserved is a no-op.
/// </summary>
public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand>
{
    private readonly IInventoryDbContext _dbContext;

    public ReserveStockCommandHandler(IInventoryDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var alreadyReserved = await _dbContext.StockReservations
            .AnyAsync(r => r.OrderId == request.OrderId, cancellationToken);
        if (alreadyReserved)
            return;

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var stockItems = await _dbContext.StockItems
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, cancellationToken);

        var requestedItems = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var reserved = StockReservationPolicy.TryReserveAll(stockItems, requestedItems, out var failureReason);

        if (!reserved)
        {
            // Nothing was mutated by the policy on the failure path, so this SaveChanges only records the outbox row.
            _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new InventoryReservationFailedIntegrationEvent
            {
                CorrelationId = request.OrderId,
                OrderId = request.OrderId,
                Reason = failureReason!
            }));

            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var reservations = requestedItems
            .Select(i => StockReservation.Create(request.OrderId, i.ProductId, i.Quantity))
            .ToList();
        _dbContext.StockReservations.AddRange(reservations);

        _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new InventoryReservedIntegrationEvent
        {
            CorrelationId = request.OrderId,
            OrderId = request.OrderId
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
