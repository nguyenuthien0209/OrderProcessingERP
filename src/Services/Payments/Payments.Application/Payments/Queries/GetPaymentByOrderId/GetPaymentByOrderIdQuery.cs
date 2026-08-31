using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Common.Interfaces;

namespace Payments.Application.Payments.Queries.GetPaymentByOrderId;

public record PaymentDto(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime ProcessedOnUtc, string? FailureReason);

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentDto?>;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    private readonly IPaymentsDbContext _dbContext;

    public GetPaymentByOrderIdQueryHandler(IPaymentsDbContext dbContext) => _dbContext = dbContext;

    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        return payment is null
            ? null
            : new PaymentDto(payment.Id, payment.OrderId, payment.Amount, payment.Status.ToString(), payment.ProcessedOnUtc, payment.FailureReason);
    }
}
