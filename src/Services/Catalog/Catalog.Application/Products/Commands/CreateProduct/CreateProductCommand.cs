using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ICatalogDbContext _dbContext;

    public CreateProductCommandHandler(ICatalogDbContext dbContext) => _dbContext = dbContext;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);
        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
