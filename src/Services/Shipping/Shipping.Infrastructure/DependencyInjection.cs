using Common.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Application.Common.Interfaces;
using Shipping.Infrastructure.Messaging.Consumers;
using Shipping.Infrastructure.Persistence;

namespace Shipping.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShippingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ShippingDb")));

        services.AddScoped<IShippingDbContext>(sp => sp.GetRequiredService<ShippingDbContext>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderConfirmedConsumer>();

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

        services.AddHostedService<OutboxProcessor<ShippingDbContext>>();

        return services;
    }
}
