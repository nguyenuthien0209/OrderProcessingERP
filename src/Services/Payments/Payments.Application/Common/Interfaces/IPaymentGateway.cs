namespace Payments.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> AuthorizeAsync(Guid orderId, decimal amount, CancellationToken cancellationToken);
}

public record PaymentGatewayResult(bool IsApproved, string? DeclineReason);
