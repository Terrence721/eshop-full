using eShop.EventBusKafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting;

public static class KafkaDependencyInjectionExtensions
{
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...",
    //     "RetryCount": 10
    //   }
    // }

    private const string SectionName = "EventBus";

    public static IEventBusBuilder AddKafkaEventBus(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // SubscriptionClientName doubles as the Kafka consumer group id. Read directly from
        // configuration here, not via IOptions<EventBusOptions> -- the container that would
        // resolve it doesn't exist yet at this point, but AddKafkaConsumer needs the group id now.
        var subscriptionClientName = builder.Configuration.GetSection(SectionName)[nameof(EventBusOptions.SubscriptionClientName)];

        builder.AddKafkaProducer<string, byte[]>(connectionName);
        builder.AddKafkaConsumer<string, byte[]>(connectionName, settings =>
        {
            settings.Config.GroupId = subscriptionClientName;
        });

        // Unlike RabbitMQ.Client, Confluent.Kafka has no built-in OpenTelemetry support, so we
        // need to add it ourselves.
        builder.Services.AddOpenTelemetry()
           .WithTracing(tracing =>
           {
               tracing.AddSource(KafkaTelemetry.ActivitySourceName);
           });

        // Options support
        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

        // Abstractions on top of the core client API
        builder.Services.AddSingleton<KafkaTelemetry>();
        builder.Services.AddSingleton<IEventSerializer, SystemTextJsonEventSerializer>();
        builder.Services.AddSingleton<KafkaEventBus>();

        // IEventBus resolves to a Decorator chain wrapping the bare KafkaEventBus:
        // resilience (outermost, so each retry attempt re-enters telemetry) then telemetry.
        builder.Services.AddSingleton<IEventBus>(sp =>
        {
            IEventBus bus = sp.GetRequiredService<KafkaEventBus>();
            bus = new TelemetryEventBusDecorator(bus, sp.GetRequiredService<KafkaTelemetry>());
            bus = new ResilientEventBusDecorator(bus, sp.GetRequiredService<IOptions<EventBusOptions>>());
            return bus;
        });

        // Start consuming messages as soon as the application starts. This resolves the same
        // KafkaEventBus singleton registered above, not the decorated IEventBus, so the
        // producer/consumer it drives in StartAsync is the one PublishAsync calls flow through.
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<KafkaEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
