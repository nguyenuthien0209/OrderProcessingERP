namespace Ordering.Application.Common.Interfaces;

/// <summary>Synchronous, read-only call to Catalog at order-creation time to validate the product and snapshot its current price.</summary>
public interface ICatalogServiceClient
{
    Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
}

public record ProductSnapshot(Guid Id, string Name, decimal Price, bool IsActive);
