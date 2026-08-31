using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Orders.Commands.CancelOrder;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Orders.Queries.GetOrderById;
using Ordering.Application.Orders.Queries.GetOrdersByCustomer;

namespace Ordering.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { orderId }, orderId);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var orders = await _sender.Send(new GetOrdersByCustomerQuery(customerId), cancellationToken);
        return Ok(orders);
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelOrderCommand(orderId, request.Reason), cancellationToken);
        return NoContent();
    }
}

public record CancelOrderRequest(string Reason);
