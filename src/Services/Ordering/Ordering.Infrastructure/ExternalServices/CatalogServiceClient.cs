using System.Net;
using System.Net.Http.Json;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Infrastructure.ExternalServices;

public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;

    public CatalogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductSnapshot>(cancellationToken: cancellationToken);
    }
}
