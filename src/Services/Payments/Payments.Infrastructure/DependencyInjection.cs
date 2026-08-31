using Common.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Common.Interfaces;
using Payments.Infrastructure.Gateway;
using Payments.Infrastructure.Messaging.Consumers;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PaymentsDb")));

        services.AddScoped<IPaymentsDbContext>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer>();

            // Retries the amount lookup a few times in case InventoryReserved is delivered before
            // this service's own OrderCreated consumer has cached the order total.
            x.AddConsumer<InventoryReservedConsumer>(cfg =>
                cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(2))));

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

        services.AddHostedService<OutboxProcessor<PaymentsDbContext>>();

        return services;
    }
}
