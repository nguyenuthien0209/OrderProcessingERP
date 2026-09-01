using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Data;

/// <summary>
/// Dev-only startup seed (same "migrate/seed on boot" shortcut the rest of the solution uses) for the API
/// scopes/resources/clients every other service needs, plus a single demo user for the identity resources.
/// </summary>
public static class SeedData
{
    private static readonly string[] ApiScopeNames =
        ["ordering.api", "inventory.api", "payments.api", "shipping.api", "catalog.api"];

    public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
    {
        var configurationContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();

        if (!configurationContext.Clients.Any())
        {
            foreach (var client in GetClients())
                configurationContext.Clients.Add(client.ToEntity());

            foreach (var resource in GetApiResources())
                configurationContext.ApiResources.Add(resource.ToEntity());

            foreach (var scope in GetApiScopes())
                configurationContext.ApiScopes.Add(scope.ToEntity());

            foreach (var resource in GetIdentityResources())
                configurationContext.IdentityResources.Add(resource.ToEntity());

            await configurationContext.SaveChangesAsync();
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByNameAsync("demo@erp.local") is null)
        {
            var demoUser = new ApplicationUser { UserName = "demo@erp.local", Email = "demo@erp.local", EmailConfirmed = true };
            await userManager.CreateAsync(demoUser, "Demo123$");
        }
    }

    private static IEnumerable<IdentityResource> GetIdentityResources() =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    ];

    private static IEnumerable<ApiScope> GetApiScopes() =>
        ApiScopeNames.Select(name => new ApiScope(name));

    private static IEnumerable<ApiResource> GetApiResources() =>
        ApiScopeNames.Select(name => new ApiResource(name) { Scopes = { name } });

    private static IEnumerable<Client> GetClients() =>
    [
        new Client
        {
            ClientId = "m2m.ordering",
            ClientName = "Ordering Service (machine-to-machine)",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("ordering-secret".Sha256()) },
            AllowedScopes = { "catalog.api" }
        },
        new Client
        {
            ClientId = "swagger",
            ClientName = "Swagger UI (interactive testing)",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("swagger-secret".Sha256()) },
            AllowedScopes = ApiScopeNames
        }
    ];
}
