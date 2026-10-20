namespace eShop.EventBusKafka;

using System.Diagnostics;

public sealed class TelemetryEventBusDecorator(IEventBus inner, KafkaTelemetry kafkaTelemetry) : IEventBus
{
    public async Task PublishAsync(IntegrationEvent @event)
    {
        var topicName = @event.GetType().Name;

        using var activity = kafkaTelemetry.ActivitySource.StartActivity($"{topicName} send", ActivityKind.Producer);

        KafkaTelemetry.SetActivityContext(activity, topicName, "send");

        try
        {
            await inner.PublishAsync(@event);
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);

            throw;
        }
    }
}
