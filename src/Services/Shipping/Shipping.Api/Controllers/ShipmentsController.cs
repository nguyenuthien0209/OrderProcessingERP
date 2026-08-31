using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipping.Application.Shipments.Queries.GetShipmentByOrderId;

namespace Shipping.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ShipmentsController(ISender sender) => _sender = sender;

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<ShipmentDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var shipment = await _sender.Send(new GetShipmentByOrderIdQuery(orderId), cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }
}
