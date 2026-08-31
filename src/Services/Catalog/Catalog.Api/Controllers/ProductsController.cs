using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(products);
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(productId), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var productId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { productId }, productId);
    }
}
