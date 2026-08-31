using Catalog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Products.Queries.GetProductById;

public record ProductDto(Guid Id, string Name, decimal Price, bool IsActive);

public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly ICatalogDbContext _dbContext;

    public GetProductByIdQueryHandler(ICatalogDbContext dbContext) => _dbContext = dbContext;

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        return product is null ? null : new ProductDto(product.Id, product.Name, product.Price, product.IsActive);
    }
}
