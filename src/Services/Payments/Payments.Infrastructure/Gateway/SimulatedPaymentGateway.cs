using Microsoft.Extensions.Configuration;
using Payments.Application.Common.Interfaces;

namespace Payments.Infrastructure.Gateway;

/// <summary>
/// Stands in for a real processor (Stripe, Adyen, ...). Declines deterministically above a configurable
/// threshold so the failure/compensation path (OrderCancelled -> inventory release) is easy to demo on purpose,
/// instead of relying on random chance.
/// </summary>
public class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly decimal _declineThreshold;

    public SimulatedPaymentGateway(IConfiguration configuration)
    {
        _declineThreshold = configuration.GetValue<decimal?>("PaymentGateway:DeclineAboveAmount") ?? 5000m;
    }

    public Task<PaymentGatewayResult> AuthorizeAsync(Guid orderId, decimal amount, CancellationToken cancellationToken)
    {
        var result = amount > _declineThreshold
            ? new PaymentGatewayResult(false, $"Amount {amount:C} exceeds the simulated authorization limit of {_declineThreshold:C}.")
            : new PaymentGatewayResult(true, null);

        return Task.FromResult(result);
    }
}
