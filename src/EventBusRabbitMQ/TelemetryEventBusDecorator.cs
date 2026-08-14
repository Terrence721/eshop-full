namespace eShop.EventBusRabbitMQ;

using System.Diagnostics;

public sealed class TelemetryEventBusDecorator(IEventBus inner, RabbitMQTelemetry rabbitMQTelemetry) : IEventBus
{
    public async Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        using var activity = rabbitMQTelemetry.ActivitySource.StartActivity($"{routingKey} publish", ActivityKind.Client);

        RabbitMQTelemetry.SetActivityContext(activity, routingKey, "publish");

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
