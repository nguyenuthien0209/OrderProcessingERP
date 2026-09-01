using Common.Outbox;
using Duende.AccessTokenManagement;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Interfaces;
using Ordering.Infrastructure.ExternalServices;
using Ordering.Infrastructure.Messaging.Consumers;
using Ordering.Infrastructure.Persistence;
using Polly;
using Polly.Extensions.Http;

namespace Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderingDb")));

        services.AddScoped<IOrderingDbContext>(sp => sp.GetRequiredService<OrderingDbContext>());

        // Ordering's HTTP call to Catalog is authenticated with a client-credentials token (scope "catalog.api"),
        // fetched from the Identity service and cached/refreshed automatically by Duende.AccessTokenManagement.
        services.AddClientCredentialsTokenManagement()
            .AddClient("catalog", client =>
            {
                client.TokenEndpoint = $"{configuration["Identity:Authority"]}/connect/token";
                client.ClientId = configuration["Identity:ClientId"];
                client.ClientSecret = configuration["Identity:ClientSecret"];
                client.Scope = configuration["Identity:Scope"];
            });

        services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:Catalog:BaseUrl"]!);
            })
            .AddClientCredentialsTokenHandler("catalog")
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryReservedConsumer>();
            x.AddConsumer<InventoryReservationFailedConsumer>();
            x.AddConsumer<PaymentAuthorizedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<OrderShippedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<OutboxProcessor<OrderingDbContext>>();

        return services;
    }
}
