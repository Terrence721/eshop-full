using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace eShop.EventBusKafka;

public class KafkaTelemetry
{
    public static string ActivitySourceName = "EventBusKafka";

    public ActivitySource ActivitySource { get; } = new(ActivitySourceName);
    public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;

    // Current OpenTelemetry messaging semantic conventions for Kafka (confirmed against the real
    // spec, not RabbitMQEventBus's older-style attribute set, which predates this revision):
    // https://opentelemetry.io/docs/specs/semconv/messaging/kafka/
    // consumerGroup is only meaningful -- and per spec only set -- on the consume side; publish
    // spans have no consumer group, so the publish call site simply omits it.
    public static void SetActivityContext(Activity? activity, string topicName, string operation, string? consumerGroup = null)
    {
        if (activity is not null)
        {
            activity.SetTag("messaging.system", "kafka");
            activity.SetTag("messaging.destination.name", topicName);
            activity.SetTag("messaging.operation.name", operation);

            if (consumerGroup is not null)
            {
                activity.SetTag("messaging.consumer.group.name", consumerGroup);
            }
        }
    }
}
