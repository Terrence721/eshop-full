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

        builder.AddKafkaProducer<string, byte[]>(connectionName, settings =>
        {
            settings.Config.BrokerAddressFamily = BrokerAddressFamily.V4;
        });
        builder.AddKafkaConsumer<string, byte[]>(connectionName, settings =>
        {
            settings.Config.GroupId = subscriptionClientName;

            // A consumer commonly starts up before any publisher has ever run (e.g. every
            // service here starts independently, in no guaranteed order) -- without this,
            // subscribing to a topic nothing has published to yet throws "Subscribed topic not
            // available" on every Consume() call until something else causes the topic to exist.
            // Live-verified: without it, Basket.API hit exactly that against a fresh broker.
            settings.Config.AllowAutoCreateTopics = true;

            // librdkafka resolves the broker's advertised-listener address (distinct from the
            // initial bootstrap address) independently, and live-verified under eShop.AppHost
            // that this doesn't reliably honor the OS-level IPv4-preference fix for "localhost"
            // even when the initial bootstrap connection does -- every service hit "N brokers are
            // down" against a container whose port is only published on IPv4. Forcing IPv4
            // explicitly here is the robust fix librdkafka itself documents for this, rather than
            // depending on OS DNS resolution order.
            settings.Config.BrokerAddressFamily = BrokerAddressFamily.V4;
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
