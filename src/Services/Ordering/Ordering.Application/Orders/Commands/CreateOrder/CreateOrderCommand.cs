using Common.Outbox;
using EventBus.Contracts;
using FluentValidation;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderCommand(Guid CustomerId, List<CreateOrderItemDto> Items) : IRequest<Guid>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("An order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

/// <summary>
/// Validates every line against Catalog, persists the order, and stages the OrderCreated integration
/// event in the same transaction (transactional outbox) — this is the event that starts the saga.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderingDbContext _dbContext;
    private readonly ICatalogServiceClient _catalogServiceClient;

    public CreateOrderCommandHandler(IOrderingDbContext dbContext, ICatalogServiceClient catalogServiceClient)
    {
        _dbContext = dbContext;
        _catalogServiceClient = catalogServiceClient;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var resolvedItems = new List<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)>();

        foreach (var item in request.Items)
        {
            var product = await _catalogServiceClient.GetProductAsync(item.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"Product {item.ProductId} does not exist.");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{product.Name}' is not available for purchase.");

            resolvedItems.Add((product.Id, product.Name, item.Quantity, product.Price));
        }

        var order = Order.Create(request.CustomerId, resolvedItems);
        _dbContext.Orders.Add(order);

        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            CorrelationId = order.Id,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items
                .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                .ToList()
        };
        _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(integrationEvent));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
