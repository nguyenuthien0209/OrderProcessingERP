using Catalog.Application.Common.Interfaces;
using Catalog.Application.Products.Queries.GetProductById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly ICatalogDbContext _dbContext;

    public GetProductsQueryHandler(ICatalogDbContext dbContext) => _dbContext = dbContext;

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.IsActive))
            .ToListAsync(cancellationToken);
    }
}
