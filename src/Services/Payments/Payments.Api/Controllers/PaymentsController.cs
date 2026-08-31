using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Payments.Queries.GetPaymentByOrderId;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender) => _sender = sender;

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<PaymentDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = await _sender.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }
}
