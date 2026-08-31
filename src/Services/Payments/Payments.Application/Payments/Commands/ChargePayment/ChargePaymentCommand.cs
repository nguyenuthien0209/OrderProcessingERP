using Common.Outbox;
using EventBus.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Common.Interfaces;
using Payments.Domain.Entities;

namespace Payments.Application.Payments.Commands.ChargePayment;

/// <summary>
/// Triggered by InventoryReserved. Looks up the amount from the local PendingOrder cache; if it
/// hasn't arrived yet (OrderCreated and InventoryReserved can race across independent queues), throws
/// so the message is retried by MassTransit's endpoint retry policy instead of failing the saga outright.
/// </summary>
public record ChargePaymentCommand(Guid OrderId) : IRequest;

public class ChargePaymentCommandHandler : IRequestHandler<ChargePaymentCommand>
{
    private readonly IPaymentsDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;

    public ChargePaymentCommandHandler(IPaymentsDbContext dbContext, IPaymentGateway paymentGateway)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
    }

    public async Task Handle(ChargePaymentCommand request, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _dbContext.Payments.AnyAsync(p => p.OrderId == request.OrderId, cancellationToken);
        if (alreadyProcessed)
            return;

        var pendingOrder = await _dbContext.PendingOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"No pending order cached yet for order {request.OrderId}; retrying.");

        var result = await _paymentGateway.AuthorizeAsync(request.OrderId, pendingOrder.TotalAmount, cancellationToken);

        if (result.IsApproved)
        {
            var payment = Payment.Authorized(request.OrderId, pendingOrder.TotalAmount);
            _dbContext.Payments.Add(payment);

            _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new PaymentAuthorizedIntegrationEvent
            {
                CorrelationId = request.OrderId,
                OrderId = request.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount
            }));
        }
        else
        {
            var reason = result.DeclineReason ?? "Payment declined.";
            _dbContext.Payments.Add(Payment.Failed(request.OrderId, pendingOrder.TotalAmount, reason));

            _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new PaymentFailedIntegrationEvent
            {
                CorrelationId = request.OrderId,
                OrderId = request.OrderId,
                Reason = reason
            }));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
