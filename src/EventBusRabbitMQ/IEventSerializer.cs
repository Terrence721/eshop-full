namespace eShop.EventBusRabbitMQ;

// Extracted from RabbitMQEventBus so the wire format can be swapped without
// touching transport code -- upstream doesn't do this, RabbitMQEventBus
// used to hardcode System.Text.Json directly.
public interface IEventSerializer
{
    byte[] Serialize(IntegrationEvent @event);

    IntegrationEvent? Deserialize(string message, Type eventType);
}
