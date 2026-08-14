using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting;

public static class RabbitMqDependencyInjectionExtensions
{
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...",
    //     "RetryCount": 10
    //   }
    // }

    private const string SectionName = "EventBus";

    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRabbitMQClient(connectionName);

        // RabbitMQ.Client doesn't have built-in support for OpenTelemetry, so we need to add it ourselves
        builder.Services.AddOpenTelemetry()
           .WithTracing(tracing =>
           {
               tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
           });

        // Options support
        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

        // Abstractions on top of the core client API
        builder.Services.AddSingleton<RabbitMQTelemetry>();
        builder.Services.AddSingleton<RabbitMQEventBus>();

        // IEventBus resolves to a Decorator chain wrapping the bare RabbitMQEventBus:
        // resilience (outermost, so each retry attempt re-enters telemetry) then telemetry.
        builder.Services.AddSingleton<IEventBus>(sp =>
        {
            IEventBus bus = sp.GetRequiredService<RabbitMQEventBus>();
            bus = new TelemetryEventBusDecorator(bus, sp.GetRequiredService<RabbitMQTelemetry>());
            bus = new ResilientEventBusDecorator(bus, sp.GetRequiredService<IOptions<EventBusOptions>>());
            return bus;
        });

        // Start consuming messages as soon as the application starts. This resolves the same
        // RabbitMQEventBus singleton registered above, not the decorated IEventBus, so the
        // connection/channel it opens in StartAsync is the one PublishAsync calls flow through.
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
