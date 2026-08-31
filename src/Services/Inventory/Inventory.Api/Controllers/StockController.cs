using Inventory.Application.Stock.Queries.GetStockByProductId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly ISender _sender;

    public StockController(ISender sender) => _sender = sender;

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<StockDto>> GetByProductId(Guid productId, CancellationToken cancellationToken)
    {
        var stock = await _sender.Send(new GetStockByProductIdQuery(productId), cancellationToken);
        return stock is null ? NotFound() : Ok(stock);
    }
}
