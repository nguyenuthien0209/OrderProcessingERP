using Common;

namespace Catalog.Domain.Entities;

public class Product : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { } // EF Core

    public static Product Create(string name, decimal price, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            IsActive = isActive
        };
    }
}
