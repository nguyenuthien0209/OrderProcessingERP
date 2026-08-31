using Common.Outbox;
using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Messaging.Consumers;
using Inventory.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("InventoryDb")));

        services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();

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

        services.AddHostedService<OutboxProcessor<InventoryDbContext>>();

        return services;
    }
}
